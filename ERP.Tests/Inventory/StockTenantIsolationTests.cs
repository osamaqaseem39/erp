using ERP.Application.Inventory;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Inventory;

public class StockTenantIsolationTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();
    private static readonly Guid Warehouse = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();

    private sealed class FixedTenant(Guid id) : ITenantProvider
    {
        public Guid? GetTenantId() => id;
    }

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Repo_FailsClosed_On_Empty_Tenant_And_Isolates()
    {
        await using var db = Db();
        var repo = new StockBalanceRepository(db);
        var id = (await repo.CreateAsync(new StockBalance
        {
            TenantId = TenantB,
            ItemId = Item,
            WarehouseId = Warehouse,
            CurrentQuantity = 10
        })).Id;

        Assert.Null(await repo.GetByIdAsync(id, TenantA));
        Assert.Null(await repo.GetByIdAsync(id, Guid.Empty));
        Assert.Empty(await repo.GetAllAsync(Guid.Empty));
        Assert.Empty(await repo.GetByWarehouseAsync(Warehouse, TenantA));
        Assert.NotNull(await repo.GetByIdAsync(id, TenantB));
    }

    [Fact]
    public async Task Service_Reads_Scoped_To_Provider_Tenant()
    {
        await using var db = Db();
        var repo = new StockBalanceRepository(db);
        await repo.CreateAsync(new StockBalance
        {
            TenantId = TenantA,
            ItemId = Item,
            WarehouseId = Warehouse,
            CurrentQuantity = 7
        });
        await repo.CreateAsync(new StockBalance
        {
            TenantId = TenantB,
            ItemId = Item,
            WarehouseId = Warehouse,
            CurrentQuantity = 99
        });

        var svcA = new StockService(repo, tenant: new FixedTenant(TenantA));
        var svcNone = new StockService(repo);

        Assert.Equal(7m, await svcA.GetStockLevelAsync(Item, Warehouse));
        Assert.Single(await svcA.GetByWarehouseAsync(Warehouse));
        Assert.Equal(0m, await svcNone.GetStockLevelAsync(Item, Warehouse));
        Assert.Empty(await svcNone.GetAllAsync());
        Assert.False(await svcNone.ReceiveStockAsync(Guid.Empty, Warehouse, [(Item, 1m)]));
    }
}
