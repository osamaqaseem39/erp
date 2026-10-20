using ERP.Api.GraphQL.Inventory;
using ERP.Application.Inventory;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Inventory;

public class GraphQLStockBalanceTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();
    private static readonly Guid Warehouse = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static StockService Svc(ERPDbContext db) =>
        new(new StockBalanceRepository(db));

    [Fact]
    public void Surface_ExposesStockBalanceFields()
    {
        var queries = typeof(InventoryQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(InventoryQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        var mutations = typeof(InventoryMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(InventoryMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetStockBalance", queries);
        Assert.Contains("GetStockBalances", queries);
        Assert.Contains("GetStockLevel", queries);
        Assert.Contains("AddStockBalance", mutations);
        Assert.Contains("UpdateStockBalance", mutations);
        Assert.Contains("DeleteStockBalance", mutations);
        Assert.Contains("AdjustStock", mutations);
    }

    [Fact]
    public async Task GraphQL_CRUD_TenantScoped_And_FailClosed()
    {
        await using var db = Db();
        var svc = Svc(db);
        var queries = new InventoryQueries();
        var mutations = new InventoryMutations();

        var created = await mutations.AddStockBalance(svc, new StockBalanceDto
        {
            ItemId = Item,
            WarehouseId = Warehouse,
            QuantityOnHand = 12m,
            QuantityReserved = 2m,
            LastUpdated = DateTime.UtcNow
        }, TenantA);

        Assert.Equal(TenantA, created.TenantId);
        Assert.Equal(12m, created.QuantityOnHand);

        Assert.NotNull(await queries.GetStockBalance(svc, created.Id, TenantA));
        Assert.Null(await queries.GetStockBalance(svc, created.Id, TenantB));
        Assert.Null(await queries.GetStockBalance(svc, created.Id, Guid.Empty));

        // GetStockLevel returns available (on-hand − reserved)
        Assert.Equal(10m, await queries.GetStockLevel(svc, Item, Warehouse, TenantA));
        Assert.Equal(0m, await queries.GetStockLevel(svc, Item, Warehouse, TenantB));
        Assert.Equal(0m, await queries.GetStockLevel(svc, Item, Warehouse, Guid.Empty));

        Assert.Single(await queries.GetStockBalances(svc, TenantA, warehouseId: Warehouse));
        Assert.Empty(await queries.GetStockBalances(svc, Guid.Empty));
        Assert.Empty(await queries.GetStockBalances(svc, TenantB));

        var updated = await mutations.UpdateStockBalance(svc, new StockBalanceDto
        {
            Id = created.Id,
            ItemId = Item,
            WarehouseId = Warehouse,
            QuantityOnHand = 20m,
            QuantityReserved = 2m,
            LastUpdated = DateTime.UtcNow
        }, TenantA);
        Assert.Equal(20m, updated.QuantityOnHand);

        Assert.False(await mutations.DeleteStockBalance(svc, created.Id, TenantB));
        Assert.False(await mutations.DeleteStockBalance(svc, created.Id, Guid.Empty));
        Assert.True(await mutations.DeleteStockBalance(svc, created.Id, TenantA));
        Assert.Null(await queries.GetStockBalance(svc, created.Id, TenantA));
    }

    [Fact]
    public async Task GraphQL_AdjustStock_TenantScoped()
    {
        await using var db = Db();
        var svc = Svc(db);
        var mutations = new InventoryMutations();
        var queries = new InventoryQueries();

        await mutations.AddStockBalance(svc, new StockBalanceDto
        {
            ItemId = Item,
            WarehouseId = Warehouse,
            QuantityOnHand = 5m,
            LastUpdated = DateTime.UtcNow
        }, TenantA);

        Assert.True(await mutations.AdjustStock(svc, new StockAdjustmentDto
        {
            WarehouseId = Warehouse,
            Details =
            [
                new StockAdjustmentDetailDto
                {
                    ItemId = Item,
                    QuantityOnHand = 5m,
                    QuantityCounted = 8m,
                    QuantityDifference = 3m
                }
            ]
        }, TenantA));

        Assert.Equal(8m, await queries.GetStockLevel(svc, Item, Warehouse, TenantA));
        Assert.False(await mutations.AdjustStock(svc, new StockAdjustmentDto
        {
            WarehouseId = Warehouse,
            Details =
            [
                new StockAdjustmentDetailDto
                {
                    ItemId = Item,
                    QuantityDifference = 1m
                }
            ]
        }, Guid.Empty));
    }
}
