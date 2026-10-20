using ERP.Application.Inventory;
using ERP.Application.Manufacturing;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Manufacturing;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Manufacturing;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Manufacturing;

public class BomBatchSerialTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Fg = Guid.NewGuid();
    private static readonly Guid Comp = Guid.NewGuid();
    private static readonly Guid Wh = Guid.NewGuid();

    private sealed class FixedTenant : ITenantProvider
    {
        public Guid? GetTenantId() => Tenant;
    }

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static StockService Stock(ERPDbContext db) =>
        new(new StockBalanceRepository(db), db: db, tenant: new FixedTenant());

    [Fact]
    public async Task Complete_Creates_Fg_Serials_And_Consumes_Component_Batch()
    {
        await using var db = Db();
        db.Items.AddRange(
            new Item
            {
                Id = Fg,
                TenantId = Tenant,
                ItemCode = "FG-S",
                ItemName = "Serialized FG",
                IsActive = true,
                IsSerialized = true
            },
            new Item
            {
                Id = Comp,
                TenantId = Tenant,
                ItemCode = "C-B",
                ItemName = "Batch Comp",
                IsActive = true,
                IsBatchTracked = true
            });
        db.Warehouses.Add(new Warehouse
        {
            Id = Wh,
            TenantId = Tenant,
            WarehouseCode = "WH-1",
            WarehouseName = "Main",
            Name = "Main",
            IsActive = true
        });
        await db.SaveChangesAsync();

        var stock = Stock(db);
        Assert.True(await stock.ReceiveStockAsync(Tenant, Wh, [(Comp, 50m)], User.ToString()));
        Assert.Equal(1, await db.BatchNumbers.CountAsync(b => b.ItemId == Comp && b.Quantity == 50m));

        var bomSvc = new BillOfMaterialService(new BillOfMaterialRepository(db));
        var bom = await bomSvc.CreateAsync(new CreateBillOfMaterialDto
        {
            Name = "Serial BOM",
            FinishedGoodItemId = Fg,
            Lines = [new BillOfMaterialLineDto { ComponentItemId = Comp, QuantityPer = 2m }]
        }, Tenant, User);
        await bomSvc.ActivateAsync(bom.Id, Tenant, User);

        var orderSvc = new ProductionOrderService(
            new ProductionOrderRepository(db),
            new ProductionOrderLineRepository(db),
            new ProductionRunRepository(db),
            new WorkCenterRepository(db),
            new BillOfMaterialRepository(db),
            db,
            stock);

        var order = await orderSvc.CreateAsync(new CreateProductionOrderDto
        {
            OrderNumber = "MO-S1",
            ItemId = Fg,
            BomId = bom.Id,
            Quantity = 5
        }, Tenant, User);
        var orderId = order.Id ?? Guid.Empty;
        await orderSvc.StartAsync(orderId, Tenant, User);

        var completed = await orderSvc.CompleteAsync(orderId, new CompleteProductionOrderDto
        {
            ActualQuantity = 5,
            WarehouseId = Wh
        }, Tenant, User);
        Assert.Equal("Completed", completed.Status);

        Assert.Equal(40m, await stock.GetStockLevelAsync(Comp, Wh));
        Assert.Equal(5m, await stock.GetStockLevelAsync(Fg, Wh));

        Assert.Equal(5, await db.SerialNumbers.CountAsync(s =>
            s.TenantId == Tenant && s.ItemId == Fg && s.Status == "Available"));
        Assert.Equal(40m, await db.BatchNumbers
            .Where(b => b.TenantId == Tenant && b.ItemId == Comp && b.IsActive)
            .SumAsync(b => b.Quantity));
    }

    [Fact]
    public async Task Issue_Fails_When_Batch_Tracked_Without_Batch_Rows()
    {
        await using var db = Db();
        db.Items.Add(new Item
        {
            Id = Comp,
            TenantId = Tenant,
            ItemCode = "C-B",
            ItemName = "Batch Comp",
            IsActive = true,
            IsBatchTracked = true
        });
        db.Warehouses.Add(new Warehouse
        {
            Id = Wh,
            TenantId = Tenant,
            WarehouseCode = "WH-1",
            WarehouseName = "Main",
            Name = "Main",
            IsActive = true
        });
        db.StockBalances.Add(new StockBalance
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            ItemId = Comp,
            WarehouseId = Wh,
            CurrentQuantity = 10,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var stock = Stock(db);
        Assert.False(await stock.IssueStockAsync(Tenant, Wh, [(Comp, 2m)], User.ToString()));
        Assert.Equal(10m, await stock.GetStockLevelAsync(Comp, Wh));
    }
}
