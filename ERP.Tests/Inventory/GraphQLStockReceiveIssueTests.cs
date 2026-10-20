using ERP.Api.GraphQL.Inventory;
using ERP.Application.Inventory;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Inventory;

public class GraphQLStockReceiveIssueTests
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

    private static StockService Svc(ERPDbContext db) =>
        new(new StockBalanceRepository(db), tenant: new FixedTenant());

    [Fact]
    public void Surface_ExposesReceiveAndIssue()
    {
        var mutations = typeof(InventoryMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(InventoryMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        Assert.Contains("ReceiveStock", mutations);
        Assert.Contains("IssueStock", mutations);
    }

    [Fact]
    public async Task GraphQL_Receive_Then_Issue()
    {
        await using var db = Db();
        db.Warehouses.Add(new Warehouse
        {
            Id = Warehouse,
            TenantId = Tenant,
            WarehouseCode = "WH-GQL",
            WarehouseName = "Main"
        });
        await db.SaveChangesAsync();

        var svc = Svc(db);
        var mutations = new InventoryMutations();

        Assert.True(await mutations.ReceiveStock(
            svc, Tenant, Warehouse,
            [new StockMovementLineDto { ItemId = Item, Quantity = 10m, UnitCost = 5m }],
            User.ToString()));

        Assert.Equal(10m, await svc.GetStockLevelAsync(Item, Warehouse));

        Assert.True(await mutations.IssueStock(
            svc, Tenant, Warehouse,
            [new StockMovementLineDto { ItemId = Item, Quantity = 3m }],
            User.ToString()));

        Assert.Equal(7m, await svc.GetStockLevelAsync(Item, Warehouse));
        Assert.False(await mutations.IssueStock(
            svc, Tenant, Warehouse,
            [new StockMovementLineDto { ItemId = Item, Quantity = 100m }],
            User.ToString()));
    }
}
