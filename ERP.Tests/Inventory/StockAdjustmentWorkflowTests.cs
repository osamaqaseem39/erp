using ERP.Application.Inventory;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Inventory;

public class StockAdjustmentWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Warehouse = Guid.NewGuid();
    private static readonly Guid ItemId = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static async Task SeedAsync(ERPDbContext db)
    {
        db.Items.Add(new Item
        {
            Id = ItemId,
            TenantId = Tenant,
            ItemCode = "SKU-1",
            ItemName = "Widget",
            CostPrice = 10,
            IsActive = true
        });
        db.Warehouses.Add(new Warehouse
        {
            Id = Warehouse,
            TenantId = Tenant,
            WarehouseCode = "WH1",
            WarehouseName = "Main",
            Name = "Main",
            IsActive = true
        });
        await db.SaveChangesAsync();
    }

    private static StockAdjustmentService Adj(ERPDbContext db) =>
        new(new StockAdjustmentRepository(db),
            new StockService(new StockBalanceRepository(db), new StockLedgerRepository(db), db));

    [Fact]
    public async Task Draft_Post_Applies_Stock_And_Blocks_Update()
    {
        await using var db = Db();
        await SeedAsync(db);
        var stock = new StockService(new StockBalanceRepository(db), new StockLedgerRepository(db), db);
        await stock.ReceiveStockAsync(Tenant, Warehouse, new[] { (ItemId, 10m, 10m) }, User.ToString());

        var svc = Adj(db);
        var id = await svc.CreateAsync(new CreateStockAdjustmentDto
        {
            WarehouseId = Warehouse,
            AdjustmentDate = DateTime.UtcNow.Date,
            Details =
            [
                new CreateStockAdjustmentDetailDto
                {
                    ItemId = ItemId,
                    QuantityOnHand = 10,
                    QuantityCounted = 7
                }
            ]
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status);

        Assert.True(await svc.PostAsync(id, Tenant, User));
        Assert.Equal("Posted", (await svc.GetByIdAsync(id, Tenant))!.Status);
        var bal = await new StockBalanceRepository(db).GetByItemAndWarehouseAsync(ItemId, Warehouse, Tenant);
        Assert.Equal(7m, bal!.CurrentQuantity);

        Assert.False(await svc.UpdateAsync(new UpdateStockAdjustmentDto
        {
            Id = id,
            WarehouseId = Warehouse,
            Details =
            [
                new UpdateStockAdjustmentDetailDto { ItemId = ItemId, QuantityOnHand = 7, QuantityCounted = 8 }
            ]
        }, Tenant, User));
        Assert.False(await svc.DeleteAsync(id, Tenant));
        Assert.False(await svc.PostAsync(id, Tenant, User));
    }

    [Fact]
    public async Task Cancel_Draft_Without_Stock_Move()
    {
        await using var db = Db();
        await SeedAsync(db);
        var svc = Adj(db);
        var id = await svc.CreateAsync(new CreateStockAdjustmentDto
        {
            WarehouseId = Warehouse,
            Details =
            [
                new CreateStockAdjustmentDetailDto
                {
                    ItemId = ItemId,
                    QuantityOnHand = 0,
                    QuantityCounted = 5
                }
            ]
        }, Tenant, User);

        Assert.True(await svc.CancelAsync(id, Tenant, User));
        Assert.Equal("Cancelled", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.False(await svc.PostAsync(id, Tenant, User));
    }
}
