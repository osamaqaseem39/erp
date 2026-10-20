using ERP.Application.Inventory;
using ERP.Application.Manufacturing;
using ERP.Domain.CRM;
using ERP.Domain.Manufacturing;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Manufacturing;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Manufacturing;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Manufacturing;

public class ManufacturingGapsWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Fg = Guid.NewGuid();
    private static readonly Guid Comp = Guid.NewGuid();
    private static readonly Guid Wh = Guid.NewGuid();
    private static readonly Guid Wc = Guid.NewGuid();

    private sealed class FixedTenant : ITenantProvider
    {
        public Guid? GetTenantId() => Tenant;
    }

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static StockService Stock(ERPDbContext db) =>
        new(new StockBalanceRepository(db), db: db, tenant: new FixedTenant());

    private static ProductionOrderService Orders(ERPDbContext db, StockService? stock = null) =>
        new(new ProductionOrderRepository(db), new ProductionOrderLineRepository(db),
            new ProductionRunRepository(db), new WorkCenterRepository(db),
            new BillOfMaterialRepository(db), db, stock);

    [Fact]
    public async Task Release_Sets_SalesOrder_Fks_And_Pending_Uses_LineId()
    {
        await using var db = Db();
        var soId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        db.SalesOrders.Add(new SalesOrder
        {
            Id = soId,
            TenantId = Tenant,
            OrderNumber = "SO-100",
            Status = "Confirmed",
            IsActive = true
        });
        db.SalesOrderLines.Add(new SalesOrderLine
        {
            Id = lineId,
            TenantId = Tenant,
            SalesOrderId = soId,
            ItemId = Fg,
            ItemName = "FG",
            Quantity = 20,
            IsActive = true
        });
        await db.SaveChangesAsync();

        var release = new ManufacturingReleaseService(db, Orders(db));
        var mo = await release.ReleaseSalesOrderLineAsync(new ReleaseSalesOrderLineDto
        {
            SalesOrderLineId = lineId,
            Quantity = 8,
            WarehouseId = Wh
        }, Tenant, User);

        Assert.Equal(soId, mo.SalesOrderId);
        Assert.Equal(lineId, mo.SalesOrderLineId);
        Assert.Equal("Released", mo.Status);
        Assert.Equal(Wh, mo.WarehouseId);

        var pending = await release.GetPendingSalesOrdersAsync(Tenant);
        Assert.Single(pending);
        Assert.Equal(8m, pending[0].ReleasedQuantity);
        Assert.Equal(12m, pending[0].PendingQuantity);
    }

    [Fact]
    public async Task Routing_Start_Creates_Ops_Qc_Then_Complete_With_Stock()
    {
        await using var db = Db();
        db.Items.AddRange(
            new Item { Id = Fg, TenantId = Tenant, ItemCode = "FG", ItemName = "FG", IsActive = true },
            new Item { Id = Comp, TenantId = Tenant, ItemCode = "C1", ItemName = "C1", IsActive = true });
        db.Warehouses.Add(new Warehouse
        {
            Id = Wh, TenantId = Tenant, WarehouseCode = "WH", WarehouseName = "Main", Name = "Main", IsActive = true
        });
        db.WorkCenters.Add(new WorkCenter
        {
            Id = Wc, TenantId = Tenant, WorkCenterCode = "WC1", WorkCenterName = "Line",
            WeeklyAvailableHours = 40, Capacity = 40, IsActive = true
        });
        await db.SaveChangesAsync();

        var stock = Stock(db);
        await stock.ReceiveStockAsync(Tenant, Wh, [(Comp, 100m)], User.ToString());

        var bomSvc = new BillOfMaterialService(new BillOfMaterialRepository(db));
        var bom = await bomSvc.CreateAsync(new CreateBillOfMaterialDto
        {
            Name = "BOM",
            FinishedGoodItemId = Fg,
            IsDefault = true,
            Lines = [new BillOfMaterialLineDto { ComponentItemId = Comp, QuantityPer = 1m }]
        }, Tenant, User);
        await bomSvc.ActivateAsync(bom.Id, Tenant, User);

        var routingSvc = new RoutingService(db);
        var routing = await routingSvc.CreateAsync(new CreateRoutingDto
        {
            RoutingNumber = "RT-1",
            Name = "Main",
            ItemId = Fg,
            IsDefault = true,
            Steps =
            [
                new CreateRoutingStepDto
                {
                    StepNumber = 1, WorkCenterId = Wc, OperationName = "Assemble", StandardTimeMinutes = 30
                }
            ]
        }, Tenant, User);
        await routingSvc.ActivateAsync(routing.Id, Tenant, User);

        var orderSvc = Orders(db, stock);
        var order = await orderSvc.CreateAsync(new CreateProductionOrderDto
        {
            OrderNumber = "MO-GAP",
            ItemId = Fg,
            BomId = bom.Id,
            RoutingId = routing.Id,
            WorkCenterId = Wc,
            WarehouseId = Wh,
            Quantity = 5
        }, Tenant, User);

        Assert.NotEmpty(order.Lines);

        await orderSvc.StartAsync(order.Id!.Value, Tenant, User);
        var ops = await orderSvc.ListOperationsAsync(order.Id!.Value, Tenant);
        Assert.Single(ops);
        Assert.Equal("InProgress", ops[0].Status);

        await orderSvc.CompleteOperationAsync(order.Id!.Value, ops[0].Id,
            new CompleteProductionOrderOperationDto { ActualTimeMinutes = 25 }, Tenant, User);
        Assert.Equal("QualityCheck", (await orderSvc.GetByIdAsync(order.Id!.Value, Tenant))!.Status);

        var qcSvc = new QualityCheckService(db);
        var qc = await qcSvc.CreateAsync(new CreateQualityCheckDto
        {
            ProductionOrderId = order.Id!.Value,
            Lines =
            [
                new QualityCheckLineDto
                {
                    ItemId = Fg, QuantityInspected = 5, QuantityPassed = 5, QuantityFailed = 0
                }
            ]
        }, Tenant, User);
        await qcSvc.PassAsync(qc.Id, null, Tenant, User);

        var completed = await orderSvc.CompleteAsync(order.Id!.Value, new CompleteProductionOrderDto
        {
            ActualQuantity = 5,
            WarehouseId = Wh
        }, Tenant, User);
        Assert.Equal("Completed", completed.Status);
        Assert.Equal(95m, await stock.GetStockLevelAsync(Comp, Wh));
        Assert.Equal(5m, await stock.GetStockLevelAsync(Fg, Wh));
    }

    [Fact]
    public async Task Scrap_Issues_Stock_Via_StockService()
    {
        await using var db = Db();
        db.Items.Add(new Item { Id = Fg, TenantId = Tenant, ItemCode = "FG", ItemName = "FG", IsActive = true });
        db.Warehouses.Add(new Warehouse
        {
            Id = Wh, TenantId = Tenant, WarehouseCode = "WH", WarehouseName = "Main", Name = "Main", IsActive = true
        });
        var poId = Guid.NewGuid();
        db.ProductionOrders.Add(new ProductionOrder
        {
            Id = poId, TenantId = Tenant, OrderNumber = "MO-S", ItemId = Fg, WarehouseId = Wh,
            Quantity = 10, Status = "InProgress", IsActive = true
        });
        await db.SaveChangesAsync();

        var stock = Stock(db);
        await stock.ReceiveStockAsync(Tenant, Wh, [(Fg, 10m)], User.ToString());

        var reasons = new ScrapReasonService(db);
        var reason = await reasons.CreateAsync(new CreateScrapReasonDto { Code = "DMG", Name = "Damage" }, Tenant, User);

        var scrapSvc = new ProductionScrapService(db, stock);
        await scrapSvc.CreateAsync(new CreateProductionScrapDto
        {
            ProductionOrderId = poId,
            Quantity = 2,
            ScrapReasonId = reason.Id,
            WarehouseId = Wh
        }, Tenant, User);

        Assert.Equal(8m, await stock.GetStockLevelAsync(Fg, Wh));
    }

    [Fact]
    public async Task Capacity_Warn_Reports_Overload_Without_Blocking()
    {
        await using var db = Db();
        db.WorkCenters.Add(new WorkCenter
        {
            Id = Wc, TenantId = Tenant, WorkCenterCode = "WC1", WorkCenterName = "Line",
            WeeklyAvailableHours = 10, Capacity = 10, IsActive = true
        });
        var routingId = Guid.NewGuid();
        db.Routings.Add(new Routing
        {
            Id = routingId, TenantId = Tenant, RoutingNumber = "R1", Name = "R", Status = "Active", IsActive = true
        });
        db.RoutingSteps.Add(new RoutingStep
        {
            Id = Guid.NewGuid(), TenantId = Tenant, RoutingId = routingId, StepNumber = 1,
            WorkCenterId = Wc, OperationName = "Cut", StandardTimeMinutes = 60, IsActive = true
        });
        db.ProductionOrders.Add(new ProductionOrder
        {
            Id = Guid.NewGuid(), TenantId = Tenant, OrderNumber = "MO-C", WorkCenterId = Wc,
            RoutingId = routingId, Quantity = 20, Status = "Released", IsActive = true,
            PlannedStartDate = DateTime.UtcNow.Date, PlannedEndDate = DateTime.UtcNow.Date.AddDays(2)
        });
        await db.SaveChangesAsync();

        var warn = await Orders(db).GetCapacityWarnAsync(Wc, Tenant);
        Assert.True(warn.Overloaded);
        Assert.Equal(10m, warn.CapacityHours);
        Assert.True(warn.LoadHours > 10m);
    }
}
