using ERP.Api.GraphQL.Manufacturing;
using ERP.Application.Manufacturing;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Manufacturing;

public class GraphQLBomProductionRunSurfaceTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Fg = Guid.NewGuid();
    private static readonly Guid Comp = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public void Surface_ExposesBomAndProductionRunParity()
    {
        var queries = typeof(ManufacturingQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(ManufacturingQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        var mutations = typeof(ManufacturingMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(ManufacturingMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetBillOfMaterial", queries);
        Assert.Contains("GetBillOfMaterials", queries);
        Assert.Contains("GetProductionRun", queries);
        Assert.Contains("GetProductionRunsByOrder", queries);

        Assert.Contains("AddBillOfMaterial", mutations);
        Assert.Contains("UpdateBillOfMaterial", mutations);
        Assert.Contains("DeleteBillOfMaterial", mutations);
        Assert.Contains("ActivateBillOfMaterial", mutations);
        Assert.Contains("ObsoleteBillOfMaterial", mutations);
        Assert.Contains("CancelProductionRun", mutations);
    }

    [Fact]
    public async Task GraphQL_Bom_Create_Activate_List_And_Obsolete()
    {
        await using var db = Db();
        db.Items.AddRange(
            new Item { Id = Fg, TenantId = Tenant, ItemCode = "FG", ItemName = "Finished", IsActive = true },
            new Item { Id = Comp, TenantId = Tenant, ItemCode = "C1", ItemName = "Comp", IsActive = true });
        await db.SaveChangesAsync();

        var bomSvc = new BillOfMaterialService(new BillOfMaterialRepository(db));
        var gql = new ManufacturingMutations();
        var queries = new ManufacturingQueries();

        var created = await gql.AddBillOfMaterial(bomSvc, new CreateBillOfMaterialDto
        {
            Name = "GQL BOM",
            FinishedGoodItemId = Fg,
            Lines = [new BillOfMaterialLineDto { ComponentItemId = Comp, QuantityPer = 1.5m }]
        }, Tenant, User);
        Assert.Equal("Draft", created.Status);

        var activated = await gql.ActivateBillOfMaterial(bomSvc, created.Id, Tenant, User);
        Assert.Equal("Active", activated.Status);

        var listed = (await queries.GetBillOfMaterials(bomSvc, Tenant, Fg)).ToList();
        Assert.Contains(listed, b => b.Id == created.Id && b.Status == "Active");

        var byId = await queries.GetBillOfMaterial(bomSvc, created.Id, Tenant);
        Assert.NotNull(byId);
        Assert.Single(byId!.Lines);

        var obsolete = await gql.ObsoleteBillOfMaterial(bomSvc, created.Id, Tenant, User);
        Assert.Equal("Obsolete", obsolete.Status);
    }

    [Fact]
    public async Task GraphQL_ProductionRun_Get_And_Cancel()
    {
        await using var db = Db();
        db.Items.Add(new Item { Id = Fg, TenantId = Tenant, ItemCode = "FG", ItemName = "Finished", IsActive = true });
        await db.SaveChangesAsync();

        var orders = new ProductionOrderRepository(db);
        var runs = new ProductionRunRepository(db);
        var orderSvc = new ProductionOrderService(
            orders,
            new ProductionOrderLineRepository(db),
            runs,
            new WorkCenterRepository(db),
            new BillOfMaterialRepository(db));
        var runSvc = new ProductionRunService(runs, orders);

        var order = await orderSvc.CreateAsync(new CreateProductionOrderDto
        {
            OrderNumber = "MO-GQL",
            ItemId = Fg,
            Quantity = 5
        }, Tenant, User);
        await orderSvc.StartAsync(order.Id!.Value, Tenant, User);

        var gql = new ManufacturingMutations();
        var queries = new ManufacturingQueries();

        var byOrder = (await queries.GetProductionRunsByOrder(runSvc, order.Id!.Value, Tenant)).ToList();
        Assert.Single(byOrder);
        var runId = byOrder[0].Id!.Value;

        var got = await queries.GetProductionRun(runSvc, runId, Tenant);
        Assert.NotNull(got);
        Assert.Equal("InProgress", got!.Status);

        Assert.True(await gql.CancelProductionRun(runSvc, runId, Tenant, User));
        Assert.Equal("Cancelled", (await db.ProductionRuns.SingleAsync(r => r.Id == runId)).Status);
    }
}
