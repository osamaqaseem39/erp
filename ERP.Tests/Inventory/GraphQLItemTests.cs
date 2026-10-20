using ERP.Api.GraphQL.Inventory;
using ERP.Application.SCM.Adapters;
using ERP.Application.SCM.Services;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Inventory;

public class GraphQLItemTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly Guid Category = Guid.NewGuid();
    private static readonly Guid Uom = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static ItemService Svc(ERPDbContext db) =>
        new(new ApplicationItemRepositoryAdapter(db));

    [Fact]
    public void Surface_ExposesItemQueriesAndMutations()
    {
        var queries = typeof(InventoryQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(InventoryQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        var mutations = typeof(InventoryMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(InventoryMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetItem", queries);
        Assert.Contains("GetItems", queries);
        Assert.Contains("GetLowStockItems", queries);
        Assert.Contains("AddItem", mutations);
        Assert.Contains("UpdateItem", mutations);
        Assert.Contains("DeleteItem", mutations);
    }

    [Fact]
    public async Task GraphQL_CRUD_TenantScoped_And_FailClosed()
    {
        await using var db = Db();
        var svc = Svc(db);
        var queries = new InventoryQueries();
        var mutations = new InventoryMutations();

        var created = await mutations.AddItem(svc, new CreateItemDto
        {
            CompanyId = Company,
            ItemCode = "SKU-A1",
            ItemName = "Widget",
            CategoryId = Category.ToString(),
            UnitOfMeasureId = Uom,
            ItemType = "Finished",
            CostPrice = 10m,
            SellingPrice = 15m,
            IsActive = true
        }, TenantA, User);

        Assert.Equal(TenantA, created.TenantId);
        Assert.Equal("SKU-A1", created.ItemCode);

        Assert.NotNull(await queries.GetItem(svc, created.Id, TenantA));
        Assert.Null(await queries.GetItem(svc, created.Id, TenantB));
        Assert.Null(await queries.GetItem(svc, created.Id, Guid.Empty));

        Assert.Single(await queries.GetItems(svc, TenantA, Company));
        Assert.Empty(await queries.GetItems(svc, Guid.Empty, Company));
        Assert.Empty(await queries.GetItems(svc, TenantB, Company));

        var updated = await mutations.UpdateItem(svc, new UpdateItemDto
        {
            Id = created.Id,
            ItemCode = "SKU-A1",
            ItemName = "Widget Pro",
            CategoryId = Category.ToString(),
            UnitOfMeasureId = Uom,
            ItemType = "Finished",
            CostPrice = 11m,
            SellingPrice = 16m,
            IsActive = true
        }, TenantA, User);
        Assert.Equal("Widget Pro", updated.ItemName);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            mutations.UpdateItem(svc, new UpdateItemDto
            {
                Id = created.Id,
                ItemCode = "SKU-A1",
                ItemName = "Hacked",
                CategoryId = Category.ToString(),
                UnitOfMeasureId = Uom,
                ItemType = "Finished",
                IsActive = true
            }, TenantB, User));

        Assert.False(await mutations.DeleteItem(svc, created.Id, TenantB));
        Assert.False(await mutations.DeleteItem(svc, created.Id, Guid.Empty));
        Assert.True(await mutations.DeleteItem(svc, created.Id, TenantA));
        Assert.Null(await queries.GetItem(svc, created.Id, TenantA));
    }

    [Fact]
    public async Task Repo_And_Service_FailClosed_On_Empty_Tenant()
    {
        await using var db = Db();
        db.Items.Add(new Item
        {
            Id = Guid.NewGuid(),
            TenantId = TenantA,
            CompanyId = Company,
            ItemCode = "X",
            ItemName = "X",
            CategoryId = Category,
            UnitOfMeasureId = Uom,
            ItemType = "Raw",
            IsActive = true
        });
        await db.SaveChangesAsync();

        var repo = new ApplicationItemRepositoryAdapter(db);
        Assert.Empty(await repo.GetAllAsync(Guid.Empty, Company));
        Assert.Null(await repo.GetByIdAsync(db.Items.First().Id, Guid.Empty));

        var svc = Svc(db);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CreateAsync(new CreateItemDto
            {
                CompanyId = Company,
                ItemCode = "Z",
                ItemName = "Z",
                CategoryId = Category.ToString(),
                UnitOfMeasureId = Uom,
                ItemType = "Raw"
            }, Guid.Empty, "user"));
    }
}
