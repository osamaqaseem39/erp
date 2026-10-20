using ERP.Api.GraphQL.Inventory;
using ERP.Application.SCM.Adapters;
using ERP.Application.SCM.Services;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Inventory;

public class GraphQLWarehouseTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid OtherTenant = Guid.NewGuid();
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static WarehouseService Svc(ERPDbContext db) =>
        new(new ApplicationWarehouseRepositoryAdapter(new WarehouseRepository(db)));

    [Fact]
    public void Surface_ExposesWarehouseQueriesAndMutations()
    {
        var queries = typeof(InventoryQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(InventoryQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        var mutations = typeof(InventoryMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(InventoryMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetWarehouse", queries);
        Assert.Contains("GetWarehouses", queries);
        Assert.Contains("SearchWarehouses", queries);

        Assert.Contains("AddWarehouse", mutations);
        Assert.Contains("UpdateWarehouse", mutations);
        Assert.Contains("DeleteWarehouse", mutations);
        Assert.Contains("ActivateWarehouse", mutations);
        Assert.Contains("DeactivateWarehouse", mutations);
    }

    [Fact]
    public async Task GraphQL_Create_Deactivate_Activate_Search_TenantScoped()
    {
        await using var db = Db();
        var svc = Svc(db);
        var queries = new InventoryQueries();
        var mutations = new InventoryMutations();

        var created = await mutations.AddWarehouse(svc, new CreateWarehouseDto
        {
            CompanyId = Company,
            WarehouseCode = "WH-GQL",
            WarehouseName = "GraphQL Dock",
            City = "Karachi",
            IsActive = true
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(Tenant, created.TenantId);
        Assert.Equal("WH-GQL", created.WarehouseCode);

        var byId = await queries.GetWarehouse(svc, created.Id, Tenant);
        Assert.Equal("GraphQL Dock", byId!.WarehouseName);
        Assert.Null(await queries.GetWarehouse(svc, created.Id, OtherTenant));
        Assert.Null(await queries.GetWarehouse(svc, created.Id, Guid.Empty));

        Assert.True(await mutations.DeactivateWarehouse(svc, created.Id, Tenant, User));
        Assert.Empty(await queries.GetWarehouses(svc, Tenant, Company, activeOnly: true));
        Assert.Single(await queries.GetWarehouses(svc, Tenant, Company));

        Assert.True(await mutations.ActivateWarehouse(svc, created.Id, Tenant, User));
        Assert.Single(await queries.GetWarehouses(svc, Tenant, Company, activeOnly: true));

        var found = (await queries.SearchWarehouses(svc, "Karachi", Tenant, Company)).ToList();
        Assert.Contains(found, w => w.Id == created.Id);
        Assert.Empty(await queries.SearchWarehouses(svc, "Karachi", OtherTenant, Company));

        Assert.True(await mutations.DeleteWarehouse(svc, created.Id, Tenant));
        Assert.Null(await queries.GetWarehouse(svc, created.Id, Tenant));
    }
}
