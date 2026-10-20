using ERP.Application.Inventory;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Inventory;

public class StockTransferWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid Creator = Guid.NewGuid();
    private static readonly Guid Approver = Guid.NewGuid();
    private static readonly Guid FromWh = Guid.NewGuid();
    private static readonly Guid ToWh = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();

    private sealed class FixedTenant : ITenantProvider
    {
        public Guid? GetTenantId() => Tenant;
    }

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static StockService Stock(ERPDbContext db) =>
        new(new StockBalanceRepository(db), db: db, tenant: new FixedTenant());

    private static StockTransferService Svc(ERPDbContext db) => new(
        new StockTransferRepository(db),
        Stock(db));

    private static async Task SeedStockAsync(ERPDbContext db, decimal qty = 10)
    {
        db.StockBalances.Add(new StockBalance
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            ItemId = Item,
            WarehouseId = FromWh,
            CurrentQuantity = qty,
            ReservedQuantity = 0
        });
        await db.SaveChangesAsync();
    }

    private static async Task<Guid> CreateDraftAsync(StockTransferService svc, decimal qty = 3)
    {
        return await svc.CreateAsync(new CreateStockTransferDto
        {
            FromWarehouseId = FromWh,
            ToWarehouseId = ToWh,
            TransferDate = DateTime.UtcNow.Date,
            Details =
            [
                new CreateStockTransferDetailDto { ItemId = Item, Quantity = qty }
            ]
        }, Tenant, Creator);
    }

    [Fact]
    public async Task Create_Always_Draft_And_Creator_Cannot_Approve()
    {
        await using var db = Db();
        await SeedStockAsync(db, qty: 10);
        var svc = Svc(db);
        var id = await CreateDraftAsync(svc);

        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.Single((await svc.GetByIdAsync(id, Tenant))!.Details);
        Assert.False(await svc.ApproveAsync(id, Tenant, Creator));
        Assert.True(await svc.ApproveAsync(id, Tenant, Approver));
        Assert.Equal("InTransit", (await svc.GetByIdAsync(id, Tenant))!.Status);
    }

    [Fact]
    public async Task Draft_Only_Editable_And_Cancel_Blocks_Complete()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await CreateDraftAsync(svc);

        Assert.True(await svc.UpdateAsync(new UpdateStockTransferDto
        {
            Id = id,
            FromWarehouseId = FromWh,
            ToWarehouseId = ToWh,
            TransferDate = DateTime.UtcNow.Date,
            Status = "Completed",
            Details =
            [
                new UpdateStockTransferDetailDto { ItemId = Item, Quantity = 1 }
            ]
        }, Tenant, Approver));
        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.Equal(1m, (await svc.GetByIdAsync(id, Tenant))!.Details[0].Quantity);

        Assert.True(await svc.CancelAsync(id, Tenant, Approver));
        Assert.Equal("Cancelled", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.False(await svc.ApproveAsync(id, Tenant, Approver));
        Assert.False(await svc.CompleteAsync(id, Tenant, Approver));
    }

    [Fact]
    public async Task Approve_Complete_Moves_Stock()
    {
        await using var db = Db();
        await SeedStockAsync(db, qty: 10);
        var svc = Svc(db);
        var stock = Stock(db);
        var id = await CreateDraftAsync(svc, qty: 4);

        Assert.False(await svc.CompleteAsync(id, Tenant, Approver)); // must approve first
        Assert.True(await svc.ApproveAsync(id, Tenant, Approver));
        Assert.True(await svc.CompleteAsync(id, Tenant, Approver));
        Assert.Equal("Completed", (await svc.GetByIdAsync(id, Tenant))!.Status);

        Assert.Equal(6m, await stock.GetStockLevelAsync(Item, FromWh));
        Assert.Equal(4m, await stock.GetStockLevelAsync(Item, ToWh));
        Assert.False(await svc.CancelAsync(id, Tenant, Approver));
    }

    [Fact]
    public async Task Approve_Fails_When_Insufficient_Stock()
    {
        await using var db = Db();
        await SeedStockAsync(db, qty: 2);
        var svc = Svc(db);
        var id = await CreateDraftAsync(svc, qty: 5);

        // Approve issues source stock; insufficient qty must fail before InTransit.
        Assert.False(await svc.ApproveAsync(id, Tenant, Approver));
        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.False(await svc.CompleteAsync(id, Tenant, Approver));
    }

    [Fact]
    public async Task Tenant_Mismatch_Blocks_Workflow()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await CreateDraftAsync(svc);
        Assert.False(await svc.ApproveAsync(id, Guid.NewGuid(), Approver));
        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.Null(await svc.GetByIdAsync(id, Guid.NewGuid()));
    }

    [Fact]
    public async Task Empty_Tenant_Fails_Closed_On_Mutations()
    {
        await using var db = Db();
        var svc = Svc(db);
        Assert.Equal(Guid.Empty, await svc.CreateAsync(new CreateStockTransferDto
        {
            FromWarehouseId = FromWh,
            ToWarehouseId = ToWh,
            TransferDate = DateTime.UtcNow.Date,
            Details = [new CreateStockTransferDetailDto { ItemId = Item, Quantity = 1 }]
        }, Guid.Empty, Creator));
        Assert.False(await svc.ApproveAsync(Guid.NewGuid(), Guid.Empty, Approver));
        Assert.False(await svc.CompleteAsync(Guid.NewGuid(), Guid.Empty, Approver));
        Assert.False(await svc.CancelAsync(Guid.NewGuid(), Guid.Empty, Approver));
    }
}
