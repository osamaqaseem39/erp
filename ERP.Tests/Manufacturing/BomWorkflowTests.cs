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

public class BomWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Fg = Guid.NewGuid();
    private static readonly Guid CompA = Guid.NewGuid();
    private static readonly Guid CompB = Guid.NewGuid();
    private static readonly Guid Wh = Guid.NewGuid();

    private sealed class FixedTenant : ITenantProvider
    {
        public Guid? GetTenantId() => Tenant;
    }

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static StockService Stock(ERPDbContext db) =>
        new(new StockBalanceRepository(db), db: db, tenant: new FixedTenant());

    private static async Task SeedItemsAndStockAsync(ERPDbContext db)
    {
        db.Items.AddRange(
            new Item { Id = Fg, TenantId = Tenant, ItemCode = "FG-1", ItemName = "Finished", IsActive = true },
            new Item { Id = CompA, TenantId = Tenant, ItemCode = "C-A", ItemName = "Comp A", IsActive = true },
            new Item { Id = CompB, TenantId = Tenant, ItemCode = "C-B", ItemName = "Comp B", IsActive = true });
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
        await stock.ReceiveStockAsync(Tenant, Wh, [(CompA, 100m), (CompB, 100m)], User.ToString());
    }

    [Fact]
    public async Task Bom_Create_Activate_Complete_Order_Issues_And_Receives()
    {
        await using var db = Db();
        await SeedItemsAndStockAsync(db);

        var bomSvc = new BillOfMaterialService(new BillOfMaterialRepository(db));
        var bom = await bomSvc.CreateAsync(new CreateBillOfMaterialDto
        {
            Name = "Widget BOM",
            FinishedGoodItemId = Fg,
            Lines =
            [
                new BillOfMaterialLineDto { ComponentItemId = CompA, QuantityPer = 2m },
                new BillOfMaterialLineDto { ComponentItemId = CompB, QuantityPer = 1m, ScrapPercent = 10m }
            ]
        }, Tenant, User);
        Assert.Equal("Draft", bom.Status);
        Assert.Equal(2, bom.Lines.Count);

        bom = await bomSvc.ActivateAsync(bom.Id, Tenant, User);
        Assert.Equal("Active", bom.Status);

        var stock = Stock(db);
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
            OrderNumber = "MO-1",
            ItemId = Fg,
            BomId = bom.Id,
            Quantity = 10
        }, Tenant, User);
        var orderId = order.Id ?? Guid.Empty;
        await orderSvc.StartAsync(orderId, Tenant, User);

        var completed = await orderSvc.CompleteAsync(orderId, new CompleteProductionOrderDto
        {
            ActualQuantity = 10,
            WarehouseId = Wh
        }, Tenant, User);
        Assert.Equal("Completed", completed.Status);
        Assert.Equal(10m, completed.CompletedQuantity);

        // A: 2*10=20; B: 1*10*1.1=11; FG receive 10
        Assert.Equal(80m, await stock.GetStockLevelAsync(CompA, Wh));
        Assert.Equal(89m, await stock.GetStockLevelAsync(CompB, Wh));
        Assert.Equal(10m, await stock.GetStockLevelAsync(Fg, Wh));
    }

    [Fact]
    public async Task Bom_Update_Only_When_Draft()
    {
        await using var db = Db();
        var bomSvc = new BillOfMaterialService(new BillOfMaterialRepository(db));
        var bom = await bomSvc.CreateAsync(new CreateBillOfMaterialDto
        {
            Name = "X",
            FinishedGoodItemId = Fg,
            Lines = [new BillOfMaterialLineDto { ComponentItemId = CompA, QuantityPer = 1m }]
        }, Tenant, User);
        await bomSvc.ActivateAsync(bom.Id, Tenant, User);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            bomSvc.UpdateAsync(new UpdateBillOfMaterialDto
            {
                Id = bom.Id,
                Name = "Y",
                FinishedGoodItemId = Fg,
                Lines = [new BillOfMaterialLineDto { ComponentItemId = CompA, QuantityPer = 2m }]
            }, Tenant, User));
    }
}
