using ERP.Application.Inventory;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Inventory;

public class ItemCategoryWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static ItemCategoryService Svc(ERPDbContext db) =>
        new(new ItemCategoryRepository(db), db);

    [Fact]
    public async Task Create_List_Update_And_SoftDelete_When_Has_Items()
    {
        await using var db = Db();
        var svc = Svc(db);

        var parentId = await svc.CreateAsync(new CreateItemCategoryDto
        {
            Code = "RAW",
            Name = "Raw Materials",
            IsActive = true
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, parentId);

        var childId = await svc.CreateAsync(new CreateItemCategoryDto
        {
            Code = "RAW-MET",
            Name = "Metals",
            ParentCategoryId = parentId,
            IsActive = true
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, childId);

        Assert.Equal(Guid.Empty, await svc.CreateAsync(new CreateItemCategoryDto
        {
            Code = "RAW",
            Name = "Dup"
        }, Tenant, User));

        var all = (await svc.GetAllAsync(Tenant)).ToList();
        Assert.Equal(2, all.Count);

        var children = (await svc.GetByParentAsync(parentId, Tenant)).ToList();
        Assert.Single(children);
        Assert.Equal("RAW-MET", children[0].Code);

        Assert.True(await svc.UpdateAsync(new UpdateItemCategoryDto
        {
            Id = childId,
            Code = "RAW-MET",
            Name = "Metals & Alloys",
            ParentCategoryId = parentId,
            IsActive = true
        }, Tenant, User));
        Assert.Equal("Metals & Alloys", (await svc.GetByIdAsync(childId, Tenant))!.Name);

        // Cannot delete parent while child exists
        Assert.False(await svc.DeleteAsync(parentId, Tenant));

        Assert.True(await svc.DeleteAsync(childId, Tenant));
        Assert.Null(await svc.GetByIdAsync(childId, Tenant));
    }
}
