using ERP.Application.Inventory;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Inventory;

public class ReservationHoldTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();
    private static readonly Guid Warehouse = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Create_IncreasesReservedQuantity_AndBlocksOversell()
    {
        await using var db = Db();
        var stockRepo = new StockBalanceRepository(db);
        var stock = new StockService(stockRepo);
        await stockRepo.CreateAsync(new StockBalance
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            ItemId = Item,
            WarehouseId = Warehouse,
            CurrentQuantity = 10,
            ReservedQuantity = 0,
            LastUpdated = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        var reservations = new StockReservationService(new StockReservationRepository(db), stock);
        var id = await reservations.CreateAsync(new CreateStockReservationDto
        {
            SalesOrderId = Guid.NewGuid(),
            Lines =
            [
                new CreateStockReservationLineDto
                {
                    ItemId = Item,
                    WarehouseId = Warehouse,
                    Quantity = 7
                }
            ]
        }, Tenant, Guid.NewGuid());

        Assert.NotEqual(Guid.Empty, id);
        var bal = await stock.GetByItemAndWarehouseAsync(Item, Warehouse, Tenant);
        Assert.NotNull(bal);
        Assert.Equal(7m, bal!.QuantityReserved);
        Assert.Equal(3m, bal.QuantityAvailable);

        var oversell = await reservations.CreateAsync(new CreateStockReservationDto
        {
            SalesOrderId = Guid.NewGuid(),
            Lines =
            [
                new CreateStockReservationLineDto
                {
                    ItemId = Item,
                    WarehouseId = Warehouse,
                    Quantity = 4
                }
            ]
        }, Tenant, Guid.NewGuid());
        Assert.Equal(Guid.Empty, oversell);

        Assert.False(await stock.IssueStockAnyWarehouseAsync(Tenant, [(Item, 4)]));
        Assert.True(await stock.IssueStockAnyWarehouseAsync(Tenant, [(Item, 3)]));
    }

    [Fact]
    public async Task Cancel_ClearsReservedQuantity()
    {
        await using var db = Db();
        var stockRepo = new StockBalanceRepository(db);
        var stock = new StockService(stockRepo);
        await stockRepo.CreateAsync(new StockBalance
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            ItemId = Item,
            WarehouseId = Warehouse,
            CurrentQuantity = 5,
            ReservedQuantity = 0,
            LastUpdated = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        var reservations = new StockReservationService(new StockReservationRepository(db), stock);
        var user = Guid.NewGuid();
        var id = await reservations.CreateAsync(new CreateStockReservationDto
        {
            SalesOrderId = Guid.NewGuid(),
            Lines =
            [
                new CreateStockReservationLineDto
                {
                    ItemId = Item,
                    WarehouseId = Warehouse,
                    Quantity = 5
                }
            ]
        }, Tenant, user);

        Assert.True(await reservations.CancelAsync(id, Tenant, user));
        var bal = await stock.GetByItemAndWarehouseAsync(Item, Warehouse, Tenant);
        Assert.Equal(0m, bal!.QuantityReserved);
        Assert.Equal(5m, bal.QuantityAvailable);
    }
}
