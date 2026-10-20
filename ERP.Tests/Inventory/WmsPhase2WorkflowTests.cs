using ERP.Application.Inventory;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Inventory;

public class WmsPhase2WorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Approver = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();
    private static readonly Guid WhFrom = Guid.NewGuid();
    private static readonly Guid WhTo = Guid.NewGuid();
    private static readonly Guid LocFrom = Guid.NewGuid();
    private static readonly Guid LocTo = Guid.NewGuid();
    private static readonly Guid LocBulk = Guid.NewGuid();
    private static readonly Guid LocPick = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static StockService Stock(ERPDbContext db) =>
        new(new StockBalanceRepository(db), new StockLedgerRepository(db), db);

    private static async Task SeedWarehousesAsync(ERPDbContext db)
    {
        db.Warehouses.AddRange(
            new Warehouse { Id = WhFrom, TenantId = Tenant, WarehouseCode = "F", WarehouseName = "From" },
            new Warehouse { Id = WhTo, TenantId = Tenant, WarehouseCode = "T", WarehouseName = "To" });
        db.WarehouseLocations.AddRange(
            new WarehouseLocation { Id = LocFrom, TenantId = Tenant, WarehouseId = WhFrom, Code = "F1", Name = "From bin", LocationType = "Pick" },
            new WarehouseLocation { Id = LocTo, TenantId = Tenant, WarehouseId = WhTo, Code = "T1", Name = "To bin", LocationType = "Pick" },
            new WarehouseLocation { Id = LocBulk, TenantId = Tenant, WarehouseId = WhFrom, Code = "B1", Name = "Bulk", LocationType = "Bulk" },
            new WarehouseLocation { Id = LocPick, TenantId = Tenant, WarehouseId = WhFrom, Code = "P1", Name = "Pick", LocationType = "Pick" });
        db.Items.Add(new Item { Id = Item, TenantId = Tenant, ItemCode = "I1", ItemName = "Item", CostPrice = 1m });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task TwoStep_Transfer_Issues_Then_Receives_And_Cancel_Reverses()
    {
        await using var db = Db();
        await SeedWarehousesAsync(db);
        var stock = Stock(db);
        Assert.True(await stock.ReceiveStockAsync(Tenant, WhFrom, [(Item, 10m, (Guid?)LocFrom)], User.ToString()));

        var transfers = new StockTransferService(new StockTransferRepository(db), stock);
        var id = await transfers.CreateAsync(new CreateStockTransferDto
        {
            FromWarehouseId = WhFrom,
            ToWarehouseId = WhTo,
            TransferDate = DateTime.UtcNow.Date,
            Details =
            [
                new CreateStockTransferDetailDto
                {
                    ItemId = Item,
                    Quantity = 4m,
                    FromLocationId = LocFrom,
                    ToLocationId = LocTo
                }
            ]
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id);

        Assert.True(await transfers.ApproveAsync(id, Tenant, Approver));
        Assert.Equal(6m, (await stock.GetByItemWarehouseLocationAsync(Item, WhFrom, LocFrom, Tenant))!.QuantityOnHand);
        Assert.Null(await stock.GetByItemWarehouseLocationAsync(Item, WhTo, LocTo, Tenant));

        Assert.True(await transfers.CompleteAsync(id, Tenant, Approver));
        Assert.Equal(4m, (await stock.GetByItemWarehouseLocationAsync(Item, WhTo, LocTo, Tenant))!.QuantityOnHand);

        // Second transfer cancelled while in transit restores source
        var id2 = await transfers.CreateAsync(new CreateStockTransferDto
        {
            FromWarehouseId = WhFrom,
            ToWarehouseId = WhTo,
            TransferDate = DateTime.UtcNow.Date,
            Details =
            [
                new CreateStockTransferDetailDto
                {
                    ItemId = Item,
                    Quantity = 2m,
                    FromLocationId = LocFrom,
                    ToLocationId = LocTo
                }
            ]
        }, Tenant, User);
        Assert.True(await transfers.ApproveAsync(id2, Tenant, Approver));
        Assert.Equal(4m, (await stock.GetByItemWarehouseLocationAsync(Item, WhFrom, LocFrom, Tenant))!.QuantityOnHand);
        Assert.True(await transfers.CancelAsync(id2, Tenant, Approver));
        Assert.Equal(6m, (await stock.GetByItemWarehouseLocationAsync(Item, WhFrom, LocFrom, Tenant))!.QuantityOnHand);
    }

    [Fact]
    public async Task Replenishment_Moves_Bulk_To_Pick()
    {
        await using var db = Db();
        await SeedWarehousesAsync(db);
        var stock = Stock(db);
        Assert.True(await stock.ReceiveStockAsync(Tenant, WhFrom, [(Item, 20m, (Guid?)LocBulk)], User.ToString()));

        var svc = new ReplenishmentTaskService(
            new ReplenishmentTaskRepository(db),
            new WarehouseLocationRepository(db),
            stock);
        var id = await svc.CreateAsync(new CreateReplenishmentTaskDto
        {
            WarehouseId = WhFrom,
            Lines =
            [
                new CreateReplenishmentTaskLineDto
                {
                    ItemId = Item,
                    Quantity = 5m,
                    FromLocationId = LocBulk,
                    ToLocationId = LocPick
                }
            ]
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id);
        Assert.True(await svc.CompleteAsync(id, Tenant, User));
        Assert.Equal(15m, (await stock.GetByItemWarehouseLocationAsync(Item, WhFrom, LocBulk, Tenant))!.QuantityOnHand);
        Assert.Equal(5m, (await stock.GetByItemWarehouseLocationAsync(Item, WhFrom, LocPick, Tenant))!.QuantityOnHand);
    }

    [Fact]
    public async Task Pack_Then_Ship_Does_Not_Touch_Stock()
    {
        await using var db = Db();
        await SeedWarehousesAsync(db);
        var stock = Stock(db);
        Assert.True(await stock.ReceiveStockAsync(Tenant, WhFrom, [(Item, 10m, (Guid?)LocPick)], User.ToString()));

        var picks = new PickListService(
            new PickListRepository(db),
            new StockBalanceRepository(db),
            new WarehouseLocationRepository(db),
            stock);
        var dispatchId = Guid.NewGuid();
        var pickId = await picks.CreateFromDispatchAsync(dispatchId, WhFrom, [(Item, 3m)], Tenant, User);
        Assert.True(await picks.CompleteAsync(pickId, null, Tenant, User));
        Assert.Equal(7m, await stock.GetStockLevelAsync(Item, WhFrom, Tenant));

        var shipments = new ShipmentService(new ShipmentRepository(db), new PackOrderRepository(db));
        var packs = new PackOrderService(new PackOrderRepository(db), new PickListRepository(db), shipments);
        var packId = await packs.CreateFromPickAsync(pickId, Tenant, User);
        Assert.NotEqual(Guid.Empty, packId);
        Assert.True(await packs.CompleteAsync(packId, Tenant, User));
        Assert.True(await packs.HasPackedForDispatchAsync(dispatchId, Tenant));

        var allShipments = (await shipments.GetAllAsync(Tenant)).ToList();
        Assert.Single(allShipments);
        Assert.True(await shipments.ShipAsync(allShipments[0].Id, new ShipShipmentDto
        {
            Carrier = "DHL",
            TrackingNumber = "TRK-1"
        }, Tenant, User));
        Assert.True(await shipments.HasShippedForDispatchAsync(dispatchId, Tenant));

        // Stock unchanged after pack/ship (already issued on pick)
        Assert.Equal(7m, await stock.GetStockLevelAsync(Item, WhFrom, Tenant));
    }
}
