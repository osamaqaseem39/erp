using ERP.Application.Inventory;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Inventory;

public class StockAverageRateTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid Warehouse = Guid.NewGuid();
    private static readonly Guid ItemId = Guid.NewGuid();

    private sealed class FixedTenant : ITenantProvider
    {
        public Guid? GetTenantId() => Tenant;
    }

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static StockService Svc(ERPDbContext db) =>
        new(new StockBalanceRepository(db), new StockLedgerRepository(db), db, new FixedTenant());

    private static async Task SeedItemAsync(ERPDbContext db, decimal costPrice = 10m)
    {
        db.Items.Add(new Item
        {
            Id = ItemId,
            TenantId = Tenant,
            ItemCode = "SKU-1",
            ItemName = "Widget",
            CostPrice = costPrice,
            SellingPrice = 20,
            ItemType = "Finished",
            CategoryId = Guid.NewGuid(),
            UnitOfMeasureId = Guid.NewGuid(),
            CompanyId = Guid.NewGuid()
        });
        db.Warehouses.Add(new Warehouse
        {
            Id = Warehouse,
            TenantId = Tenant,
            WarehouseCode = "WH1",
            WarehouseName = "Main",
            CompanyId = Guid.NewGuid()
        });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task AverageRate_Weighted_From_Receipts_And_Issues()
    {
        await using var db = Db();
        await SeedItemAsync(db, costPrice: 0);
        var svc = Svc(db);

        Assert.True(await svc.ReceiveStockAsync(Tenant, Warehouse, new[]
        {
            (ItemId, 10m, 10m),
            (ItemId, 10m, 20m)
        }, Tenant.ToString()));

        // WAC after receipts: (10*10 + 10*20) / 20 = 15
        var mid = await svc.CalculateAverageRateAsync(Tenant, new CalculateStockAverageRateRequest
        {
            ItemId = ItemId,
            WarehouseId = Warehouse
        });
        Assert.Equal(15m, mid.AverageRate);
        Assert.Equal(20m, mid.QuantityOnHand);

        Assert.True(await svc.IssueStockAsync(Tenant, Warehouse, new[] { (ItemId, 5m) }, Tenant.ToString()));

        var after = await svc.CalculateAverageRateAsync(Tenant, new CalculateStockAverageRateRequest
        {
            ItemId = ItemId,
            WarehouseId = Warehouse,
            UpdateItemCostPrice = true
        });
        Assert.Equal(15m, after.AverageRate);
        Assert.Equal(15m, after.QuantityOnHand);
        Assert.Equal(225m, after.TotalValue); // 15 * 15
        Assert.True(after.CostPriceUpdated);

        var item = await db.Items.AsNoTracking().FirstAsync(i => i.Id == ItemId);
        Assert.Equal(15m, item.CostPrice);

        var ledger = (await svc.GetStockLedgerAsync(ItemId, Tenant, Warehouse)).ToList();
        Assert.True(ledger.Count >= 3);
    }

    [Fact]
    public async Task Transfer_Moves_Quantity_And_Writes_Ledger()
    {
        await using var db = Db();
        await SeedItemAsync(db);
        var wh2 = Guid.NewGuid();
        db.Warehouses.Add(new Warehouse
        {
            Id = wh2, TenantId = Tenant, WarehouseCode = "WH2", WarehouseName = "Secondary",
            CompanyId = Guid.NewGuid()
        });
        await db.SaveChangesAsync();
        var svc = Svc(db);

        await svc.ReceiveStockAsync(Tenant, Warehouse, new[] { (ItemId, 8m, 12m) }, "u");
        Assert.True(await svc.TransferStockAsync(new StockTransferDto
        {
            TenantId = Tenant,
            FromWarehouseId = Warehouse,
            ToWarehouseId = wh2,
            CreatedBy = "u",
            Details =
            [
                new StockTransferDetailDto { ItemId = ItemId, Quantity = 3m }
            ]
        }));

        Assert.Equal(5m, await svc.GetStockLevelAsync(ItemId, Warehouse));
        Assert.Equal(3m, await svc.GetStockLevelAsync(ItemId, wh2));
    }
}
