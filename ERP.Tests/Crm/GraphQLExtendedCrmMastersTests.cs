using ERP.Api.GraphQL.CRM;
using ERP.Application.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.CRM;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.CRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.CRM;

public class GraphQLExtendedCrmMastersTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid Other = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private sealed class FixedTenant : ITenantProvider
    {
        public Guid? GetTenantId() => Tenant;
    }

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public void Surface_ExposesExtendedMasters()
    {
        var queries = typeof(CRMQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(CRMQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        var mutations = typeof(CRMMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(CRMMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetSalesman", queries);
        Assert.Contains("GetSalesmen", queries);
        Assert.Contains("GetCustomerGroup", queries);
        Assert.Contains("GetCustomerGroups", queries);
        Assert.Contains("GetPriceList", queries);
        Assert.Contains("GetPriceLists", queries);
        Assert.Contains("GetSalesTerritory", queries);
        Assert.Contains("GetSalesTerritories", queries);

        Assert.Contains("AddSalesman", mutations);
        Assert.Contains("UpdateSalesman", mutations);
        Assert.Contains("DeleteSalesman", mutations);
        Assert.Contains("AddCustomerGroup", mutations);
        Assert.Contains("AddPriceList", mutations);
        Assert.Contains("AddSalesTerritory", mutations);
    }

    [Fact]
    public async Task GraphQL_Masters_Crud_TenantFailClosed()
    {
        await using var db = Db();
        var tenant = new FixedTenant();
        var queries = new CRMQueries();
        var mutations = new CRMMutations();

        var salesmen = new SalesmanService(new SalesmanRepository(db, tenant));
        var groups = new CustomerGroupService(new CustomerGroupRepository(db, tenant));
        var prices = new PriceListService(new PriceListRepository(db, tenant));
        var territories = new SalesTerritoryService(new SalesTerritoryRepository(db, tenant));

        var smId = await mutations.AddSalesman(salesmen, new CreateSalesmanDto
        {
            FullName = "Sara Ali",
            SalesmanCode = "SM-GQL"
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, smId);
        Assert.Equal("Sara Ali", (await queries.GetSalesman(salesmen, smId, Tenant))!.FullName);
        Assert.Null(await queries.GetSalesman(salesmen, smId, Other));
        Assert.Empty(await queries.GetSalesmen(salesmen, Guid.Empty));

        Assert.True(await mutations.UpdateSalesman(salesmen, new UpdateSalesmanDto
        {
            Id = smId,
            FullName = "Sara Ali Khan"
        }, Tenant, User));
        Assert.False(await mutations.UpdateSalesman(salesmen, new UpdateSalesmanDto
        {
            Id = smId,
            FullName = "Hacked"
        }, Other, User));

        var groupId = await mutations.AddCustomerGroup(groups, new CreateCustomerGroupDto
        {
            GroupName = "Retail",
            DiscountPercentage = 5m
        }, Tenant, User);
        Assert.Contains(await queries.GetCustomerGroups(groups, Tenant), g => g.Id == groupId);

        var priceId = await mutations.AddPriceList(prices, new CreatePriceListDto
        {
            PriceListName = "Standard",
            ValidFrom = DateTime.UtcNow.Date,
            IsDefault = true
        }, Tenant, User);
        Assert.Equal("Standard", (await queries.GetPriceList(prices, priceId, Tenant))!.PriceListName);

        var terrId = await mutations.AddSalesTerritory(territories, new CreateSalesTerritoryDto
        {
            TerritoryName = "North"
        }, Tenant, User);
        Assert.Contains(await queries.GetSalesTerritories(territories, Tenant), t => t.Id == terrId);

        Assert.True(await mutations.DeleteSalesTerritory(territories, terrId, Tenant));
        Assert.True(await mutations.DeletePriceList(prices, priceId, Tenant));
        Assert.True(await mutations.DeleteCustomerGroup(groups, groupId, Tenant));
        Assert.True(await mutations.DeleteSalesman(salesmen, smId, Tenant));
        Assert.Null(await queries.GetSalesman(salesmen, smId, Tenant));
    }
}
