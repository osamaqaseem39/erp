using ERP.Api.GraphQL.Inventory;
using ERP.Application.SCM.Services;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Inventory;

public class GraphQLValuationMethodTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public void Surface_ExposesValuationMethod()
    {
        var queries = typeof(InventoryQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(InventoryQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        var mutations = typeof(InventoryMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(InventoryMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetInventoryValuationMethod", queries);
        Assert.Contains("GetInventoryValuationMethods", queries);
        Assert.Contains("AddInventoryValuationMethod", mutations);
        Assert.Contains("UpdateInventoryValuationMethod", mutations);
        Assert.Contains("DeleteInventoryValuationMethod", mutations);
    }

    [Fact]
    public async Task GraphQL_ValuationMethod_Crud()
    {
        await using var db = Db();
        var mutations = new InventoryMutations();
        var queries = new InventoryQueries();
        var service = new InventoryValuationMethodService(new InventoryValuationMethodRepository(db));

        var created = await mutations.AddInventoryValuationMethod(service, new InventoryValuationMethodDto
        {
            MethodCode = "FIFO",
            MethodName = "First In First Out",
            Description = "FIFO costing",
            IsActive = true
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("FIFO", (await queries.GetInventoryValuationMethod(service, created.Id, Tenant))!.MethodCode);

        created.MethodName = "FIFO Costing";
        Assert.Equal("FIFO Costing", (await mutations.UpdateInventoryValuationMethod(service, created, Tenant, User)).MethodName);

        Assert.Contains(
            await queries.GetInventoryValuationMethods(service, Tenant),
            m => m.Id == created.Id);

        Assert.True(await mutations.DeleteInventoryValuationMethod(service, created.Id, Tenant));
        Assert.Null(await queries.GetInventoryValuationMethod(service, created.Id, Tenant));
    }
}
