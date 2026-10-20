using ERP.Application.Inventory;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Inventory;

public class WmsBinStockWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();
    private static readonly Guid Warehouse = Guid.NewGuid();
    private static readonly Guid LocPick = Guid.NewGuid();
    private static readonly Guid LocBulk = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static async Task SeedAsync(ERPDbContext db)
    {
        db.Warehouses.Add(new Warehouse
        {
            Id = Warehouse,
            TenantId = Tenant,
            WarehouseCode = "WH1",
            WarehouseName = "Main"
        });
        db.WarehouseLocations.AddRange(
            new WarehouseLocation
            {
                Id = LocPick,
                TenantId = Tenant,
                WarehouseId = Warehouse,
                Code = "P-01",
                Name = "Pick face",
                LocationType = "Pick"
            },
            new WarehouseLocation
            {
                Id = LocBulk,
                TenantId = Tenant,
                WarehouseId = Warehouse,
                Code = "B-01",
                Name = "Bulk",
                LocationType = "Bulk"
            });
        db.Items.Add(new Item
        {
            Id = Item,
            TenantId = Tenant,
            ItemCode = "SKU-1",
            ItemName = "Widget",
            CostPrice = 10m
        });
        await db.SaveChangesAsync();
    }

    private static StockService Stock(ERPDbContext db) =>
        new(new StockBalanceRepository(db), new StockLedgerRepository(db), db);

    [Fact]
    public async Task Receive_And_Issue_At_Bin_Are_Isolated()
    {
        await using var db = Db();
        await SeedAsync(db);
        var stock = Stock(db);

        Assert.True(await stock.ReceiveStockAsync(
            Tenant, Warehouse,
            [(Item, 8m, (Guid?)LocBulk), (Item, 2m, (Guid?)LocPick)],
            User.ToString()));

        Assert.Equal(10m, await stock.GetStockLevelAsync(Item, Warehouse, Tenant));
        Assert.Equal(8m, (await stock.GetByItemWarehouseLocationAsync(Item, Warehouse, LocBulk, Tenant))!.QuantityOnHand);
        Assert.Equal(2m, (await stock.GetByItemWarehouseLocationAsync(Item, Warehouse, LocPick, Tenant))!.QuantityOnHand);

        Assert.True(await stock.IssueStockAsync(
            Tenant, Warehouse,
            [(Item, 2m, (Guid?)LocPick)],
            User.ToString()));

        Assert.Equal(0m, (await stock.GetByItemWarehouseLocationAsync(Item, Warehouse, LocPick, Tenant))!.QuantityOnHand);
        Assert.Equal(8m, (await stock.GetByItemWarehouseLocationAsync(Item, Warehouse, LocBulk, Tenant))!.QuantityOnHand);
        Assert.Equal(8m, await stock.GetStockLevelAsync(Item, Warehouse, Tenant));
    }

    [Fact]
    public async Task Putaway_Complete_Receives_Into_Actual_Location()
    {
        await using var db = Db();
        await SeedAsync(db);
        var stock = Stock(db);
        var putaway = new PutawayTaskService(
            new PutawayTaskRepository(db),
            new WarehouseLocationRepository(db),
            stock);

        var id = await putaway.CreateAsync(new CreatePutawayTaskDto
        {
            WarehouseId = Warehouse,
            ReferenceDocType = "GRN",
            ReferenceDocId = Guid.NewGuid(),
            Lines =
            [
                new CreatePutawayTaskLineDto { ItemId = Item, Quantity = 5m, SuggestedLocationId = LocBulk }
            ]
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id);

        Assert.True(await putaway.CompleteAsync(id, new CompletePutawayTaskDto
        {
            Lines = [new CompletePutawayLineDto { LineId = (await putaway.GetByIdAsync(id, Tenant))!.Lines[0].Id, ActualLocationId = LocPick }]
        }, Tenant, User));

        Assert.Equal(5m, (await stock.GetByItemWarehouseLocationAsync(Item, Warehouse, LocPick, Tenant))!.QuantityOnHand);
        Assert.Null(await stock.GetByItemWarehouseLocationAsync(Item, Warehouse, LocBulk, Tenant));
    }

    [Fact]
    public async Task Pick_Complete_Issues_From_Location_And_Blocks_Double_Use()
    {
        await using var db = Db();
        await SeedAsync(db);
        var stock = Stock(db);
        Assert.True(await stock.ReceiveStockAsync(
            Tenant, Warehouse, [(Item, 10m, (Guid?)LocPick)], User.ToString()));

        var picks = new PickListService(
            new PickListRepository(db),
            new StockBalanceRepository(db),
            new WarehouseLocationRepository(db),
            stock);

        var dispatchId = Guid.NewGuid();
        var pickId = await picks.CreateFromDispatchAsync(
            dispatchId, Warehouse, [(Item, 4m)], Tenant, User);
        Assert.NotEqual(Guid.Empty, pickId);

        Assert.True(await picks.CompleteAsync(pickId, null, Tenant, User));
        Assert.Equal(6m, await stock.GetStockLevelAsync(Item, Warehouse, Tenant));
        Assert.True(await picks.HasCompletedPickForDispatchAsync(dispatchId, Tenant));

        // Completing again must fail (already Completed).
        Assert.False(await picks.CompleteAsync(pickId, null, Tenant, User));
    }

    [Fact]
    public async Task Putaway_Create_FailsClosed_OnEmptyTenant()
    {
        await using var db = Db();
        await SeedAsync(db);
        var putaway = new PutawayTaskService(
            new PutawayTaskRepository(db),
            new WarehouseLocationRepository(db),
            new StockService(new StockBalanceRepository(db), db: db));

        var id = await putaway.CreateAsync(new CreatePutawayTaskDto
        {
            WarehouseId = Warehouse,
            Lines = [new CreatePutawayTaskLineDto { ItemId = Item, Quantity = 1m }]
        }, Guid.Empty, User);

        Assert.Equal(Guid.Empty, id);
        Assert.False(await putaway.StartAsync(Guid.NewGuid(), Guid.Empty, User));
        Assert.False(await putaway.CompleteAsync(Guid.NewGuid(), null, Guid.Empty, User));
    }
}
