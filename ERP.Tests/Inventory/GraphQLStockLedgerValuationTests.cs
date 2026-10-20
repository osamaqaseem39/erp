using ERP.Api.GraphQL.Inventory;
using ERP.Application.Inventory;
using ERP.Application.SCM.Adapters;
using ERP.Application.SCM.Services;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Inventory;

public class GraphQLStockLedgerValuationTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();
    private static readonly Guid Warehouse = Guid.NewGuid();

    private sealed class FixedTenant : ITenantProvider
    {
        public Guid? GetTenantId() => Tenant;
    }

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static StockService Stock(ERPDbContext db) =>
        new(new StockBalanceRepository(db), db: db, tenant: new FixedTenant());

    private static InventoryService Inventory(ERPDbContext db, StockService stock) =>
        new(stock,
            new StockTransferService(new StockTransferRepository(db), stock),
            new StockBalanceRepository(db),
            new ItemService(new ApplicationItemRepositoryAdapter(db)));

    [Fact]
    public void Surface_ExposesLedgerAverageRateValuation()
    {
        var queries = typeof(InventoryQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(InventoryQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetStockLedger", queries);
        Assert.Contains("GetStockAverageRate", queries);
        Assert.Contains("CalculateStockAverageRate", queries);
        Assert.Contains("GetInventoryValuation", queries);
    }

    [Fact]
    public async Task GraphQL_Ledger_AverageRate_Valuation_AfterReceive()
    {
        await using var db = Db();
        db.Items.Add(new Item
        {
            Id = Item,
            TenantId = Tenant,
            ItemCode = "SKU-GQL",
            ItemName = "Widget",
            CostPrice = 0m,
            IsActive = true
        });
        db.Warehouses.Add(new Warehouse
        {
            Id = Warehouse,
            TenantId = Tenant,
            WarehouseCode = "WH-GQL",
            WarehouseName = "Main",
            Name = "Main",
            IsActive = true
        });
        await db.SaveChangesAsync();

        var stock = Stock(db);
        var inventory = Inventory(db, stock);
        var queries = new InventoryQueries();
        var mutations = new InventoryMutations();

        Assert.True(await mutations.ReceiveStock(
            stock, Tenant, Warehouse,
            [new StockMovementLineDto { ItemId = Item, Quantity = 10m, UnitCost = 5m }],
            User.ToString()));

        var ledger = (await queries.GetStockLedger(stock, Item, Tenant, Warehouse)).ToList();
        Assert.NotEmpty(ledger);
        Assert.All(ledger, l => Assert.Equal(Item, l.ItemId));

        var avg = await queries.GetStockAverageRate(stock, Tenant, Item, Warehouse);
        Assert.Equal(5m, avg.AverageRate);
        Assert.Equal(10m, avg.QuantityOnHand);

        var calculated = await queries.CalculateStockAverageRate(stock, new CalculateStockAverageRateRequest
        {
            ItemId = Item,
            WarehouseId = Warehouse
        }, Tenant);
        Assert.Equal(5m, calculated.AverageRate);

        var valuation = await queries.GetInventoryValuation(inventory, Tenant, Warehouse);
        Assert.Contains(valuation.Lines, l => l.ItemId == Item && l.QuantityOnHand == 10m);
        Assert.Equal(50m, valuation.TotalValue);

        Assert.Empty(await queries.GetStockLedger(stock, Guid.Empty, Tenant));
        Assert.Equal(0m, (await queries.GetStockAverageRate(stock, Tenant, Guid.Empty)).AverageRate);
    }
}
