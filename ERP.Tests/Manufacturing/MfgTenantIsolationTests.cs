using ERP.Application.Manufacturing;
using ERP.Domain.Manufacturing;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Manufacturing;

public class MfgTenantIsolationTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();
    private static readonly Guid UserA = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task WorkCenter_Other_Tenant_Cannot_Get_Or_Delete()
    {
        await using var db = Db();
        var svc = new WorkCenterService(new WorkCenterRepository(db));
        var created = await svc.CreateAsync(new CreateWorkCenterDto
        {
            WorkCenterCode = "WC-B",
            WorkCenterName = "Secret"
        }, TenantB, UserA);

        Assert.Null(await svc.GetByIdAsync(created.Id!.Value, TenantA));
        Assert.False(await svc.DeleteAsync(created.Id!.Value, TenantA));
        Assert.Empty(await svc.GetAllAsync(TenantA));
        Assert.Single(await svc.GetAllAsync(TenantB));
    }

    [Fact]
    public async Task ProductionOrder_Other_Tenant_Cannot_Get_Start_Or_Delete()
    {
        await using var db = Db();
        var orderSvc = new ProductionOrderService(
            new ProductionOrderRepository(db),
            new ProductionOrderLineRepository(db),
            new ProductionRunRepository(db),
            new WorkCenterRepository(db),
            new BillOfMaterialRepository(db));

        var order = await orderSvc.CreateAsync(new CreateProductionOrderDto
        {
            OrderNumber = "MO-B",
            ItemId = Guid.NewGuid(),
            Quantity = 5
        }, TenantB, UserA);

        Assert.Null(await orderSvc.GetByIdAsync(order.Id!.Value, TenantA));
        Assert.False(await orderSvc.DeleteAsync(order.Id!.Value, TenantA));
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            orderSvc.StartAsync(order.Id!.Value, TenantA, UserA));

        Assert.Equal("Draft", (await db.ProductionOrders.SingleAsync(o => o.Id == order.Id)).Status);
    }

    [Fact]
    public async Task ProductionRun_Other_Tenant_Cannot_Complete()
    {
        await using var db = Db();
        var orders = new ProductionOrderRepository(db);
        var runs = new ProductionRunRepository(db);
        var orderSvc = new ProductionOrderService(
            orders, new ProductionOrderLineRepository(db), runs,
            new WorkCenterRepository(db), new BillOfMaterialRepository(db));
        var runSvc = new ProductionRunService(runs, orders);

        var order = await orderSvc.CreateAsync(new CreateProductionOrderDto
        {
            OrderNumber = "MO-R",
            ItemId = Guid.NewGuid(),
            Quantity = 2
        }, TenantB, UserA);
        await orderSvc.StartAsync(order.Id!.Value, TenantB, UserA);
        var run = (await runSvc.GetByOrderAsync(order.Id!.Value, TenantB)).Single();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            runSvc.CompleteAsync(run.Id!.Value, 2, 0, TenantA, UserA));
        Assert.Equal("InProgress", (await db.ProductionRuns.SingleAsync(r => r.Id == run.Id)).Status);
    }

    [Fact]
    public async Task Empty_Tenant_Fails_Closed()
    {
        await using var db = Db();
        db.WorkCenters.Add(new WorkCenter
        {
            Id = Guid.NewGuid(),
            TenantId = TenantA,
            WorkCenterCode = "WC",
            WorkCenterName = "Main",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        Assert.Empty(await new WorkCenterRepository(db).GetAllAsync(Guid.Empty));
        Assert.Null(await new WorkCenterRepository(db).GetByIdAsync(db.WorkCenters.First().Id, Guid.Empty));
    }
}
