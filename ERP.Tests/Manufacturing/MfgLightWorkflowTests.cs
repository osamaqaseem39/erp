using ERP.Application.Manufacturing;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Manufacturing;

public class MfgLightWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task WorkCenter_Create_And_List()
    {
        await using var db = Db();
        var svc = new WorkCenterService(new WorkCenterRepository(db));

        var created = await svc.CreateAsync(new CreateWorkCenterDto
        {
            WorkCenterCode = "WC-01",
            WorkCenterName = "Assembly",
            Capacity = 8,
            CapacityUnit = "Hours"
        }, Tenant, User);

        Assert.NotEqual(Guid.Empty, created.Id!.Value);
        Assert.Equal("WC-01", created.Code);
        Assert.Equal("Assembly", created.Name);

        var all = (await svc.GetAllAsync(Tenant)).ToList();
        Assert.Single(all);
        Assert.Equal(created.Id, all[0].Id);
    }

    [Fact]
    public async Task ProductionOrder_Start_Creates_Run_And_Order_Complete_Closes_Order()
    {
        await using var db = Db();
        var orders = new ProductionOrderRepository(db);
        var runs = new ProductionRunRepository(db);
        var lines = new ProductionOrderLineRepository(db);
        var centers = new WorkCenterRepository(db);
        var orderSvc = new ProductionOrderService(orders, lines, runs, centers, new BillOfMaterialRepository(db), db);
        var runSvc = new ProductionRunService(runs, orders);

        var order = await orderSvc.CreateAsync(new CreateProductionOrderDto
        {
            OrderNumber = "PO-100",
            ItemId = Item,
            Quantity = 10,
            Priority = "High"
        }, Tenant, User);
        Assert.Equal("Draft", order.Status);

        await orderSvc.StartAsync(order.Id!.Value, Tenant, User);
        Assert.Equal("InProgress", (await orderSvc.GetByIdAsync(order.Id!.Value, Tenant))!.Status);

        var activeRuns = (await runSvc.GetByOrderAsync(order.Id!.Value, Tenant)).ToList();
        Assert.Single(activeRuns);
        Assert.Equal("InProgress", activeRuns[0].Status);

        var completedRun = await runSvc.CompleteAsync(
            activeRuns[0].Id!.Value, quantityProduced: 10, quantityRejected: 0, Tenant, User);
        Assert.Equal("Completed", completedRun.Status);

        // Run complete must not close the order (stock path lives on order Complete).
        Assert.Equal("InProgress", (await orderSvc.GetByIdAsync(order.Id!.Value, Tenant))!.Status);

        var closed = await orderSvc.CompleteAsync(order.Id!.Value, new CompleteProductionOrderDto
        {
            ActualQuantity = 10
        }, Tenant, User);
        Assert.Equal("Completed", closed.Status);
    }

    [Fact]
    public async Task ProductionOrder_Start_Rejects_NonDraft()
    {
        await using var db = Db();
        var svc = new ProductionOrderService(
            new ProductionOrderRepository(db),
            new ProductionOrderLineRepository(db),
            new ProductionRunRepository(db),
            new WorkCenterRepository(db),
            new BillOfMaterialRepository(db),
            db);

        var order = await svc.CreateAsync(new CreateProductionOrderDto
        {
            OrderNumber = "PO-200",
            ItemId = Item,
            Quantity = 5
        }, Tenant, User);

        await svc.StartAsync(order.Id!.Value, Tenant, User);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.StartAsync(order.Id!.Value, Tenant, User));
    }
}
