using ERP.Application.Inventory;
using ERP.Application.SCM.Services;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Inventory;

public class InventoryServiceFacadeTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();
    private static readonly Guid Wh = Guid.NewGuid();
    private static readonly Guid Wh2 = Guid.NewGuid();

    private sealed class FixedTenant : ITenantProvider
    {
        public Guid? GetTenantId() => Tenant;
    }

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static async Task SeedAsync(ERPDbContext db)
    {
        db.Items.Add(new Item
        {
            Id = Item,
            TenantId = Tenant,
            ItemCode = "SKU-1",
            ItemName = "Widget",
            CostPrice = 10m,
            IsActive = true,
            ReorderLevel = 5
        });
        db.Warehouses.AddRange(
            new Warehouse
            {
                Id = Wh,
                TenantId = Tenant,
                WarehouseCode = "WH1",
                WarehouseName = "Main",
                Name = "Main",
                IsActive = true
            },
            new Warehouse
            {
                Id = Wh2,
                TenantId = Tenant,
                WarehouseCode = "WH2",
                WarehouseName = "Secondary",
                Name = "Secondary",
                IsActive = true
            });
        await db.SaveChangesAsync();
    }

    private static InventoryService Svc(ERPDbContext db)
    {
        var balances = new StockBalanceRepository(db);
        var stock = new StockService(balances, db: db, tenant: new FixedTenant());
        var transfers = new StockTransferService(new StockTransferRepository(db), stock);
        var items = new ItemService(new ERP.Application.SCM.Adapters.ApplicationItemRepositoryAdapter(db));
        return new InventoryService(stock, transfers, balances, items);
    }

    [Fact]
    public async Task Balance_Transfer_Complete_And_Valuation()
    {
        await using var db = Db();
        await SeedAsync(db);
        var svc = Svc(db);
        var stock = new StockService(new StockBalanceRepository(db), db: db, tenant: new FixedTenant());

        Assert.True(await stock.ReceiveStockAsync(Tenant, Wh, [(Item, 20m)], User.ToString()));

        var bal = await svc.GetStockBalanceAsync(Item, Wh, Tenant);
        Assert.NotNull(bal);
        Assert.Equal(20m, bal!.QuantityOnHand);

        Assert.True(await svc.TransferStockAsync(new CreateStockTransferDto
        {
            FromWarehouseId = Wh,
            ToWarehouseId = Wh2,
            TransferDate = DateTime.UtcNow.Date,
            Details = [new CreateStockTransferDetailDto { ItemId = Item, Quantity = 4m }]
        }, Tenant, User.ToString()));

        var transfers = (await svc.GetStockTransfersAsync(Tenant, Guid.Empty)).ToList();
        Assert.Single(transfers);
        var transferId = transfers[0].Id!.Value;

        var approver = Guid.NewGuid();
        Assert.True(await svc.UpdateTransferStatusAsync(transferId, Tenant, "Approved", approver.ToString(), approver.ToString()));
        Assert.True(await svc.UpdateTransferStatusAsync(transferId, Tenant, "Completed", User.ToString(), User.ToString()));

        Assert.Equal(16m, (await svc.GetStockBalanceAsync(Item, Wh, Tenant))!.QuantityOnHand);
        Assert.Equal(4m, (await svc.GetStockBalanceAsync(Item, Wh2, Tenant))!.QuantityOnHand);

        var valuation = await svc.GetInventoryValuationAsync(Tenant, Wh);
        Assert.Single(valuation.Lines);
        Assert.Equal(16m, valuation.TotalQuantity);
        Assert.True(valuation.TotalValue > 0);
    }

    [Fact]
    public async Task Adjust_Increase_And_Decrease()
    {
        await using var db = Db();
        await SeedAsync(db);
        var svc = Svc(db);

        Assert.True(await svc.AdjustInventoryAsync(
            Item, Wh, Tenant, "Increase", 5m, "cycle count", null, User.ToString()));
        Assert.Equal(5m, (await svc.GetStockBalanceAsync(Item, Wh, Tenant))!.QuantityOnHand);

        Assert.True(await svc.AdjustInventoryAsync(
            Item, Wh, Tenant, "Decrease", 2m, "damage", null, User.ToString()));
        Assert.Equal(3m, (await svc.GetStockBalanceAsync(Item, Wh, Tenant))!.QuantityOnHand);
    }
}
