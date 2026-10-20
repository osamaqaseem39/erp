using ERP.Application.SCM.Services;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Inventory;

public class ItemGroupTenantIsolationTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static ItemGroupService Svc(ERPDbContext db) =>
        new(new ItemGroupRepository(db));

    [Fact]
    public async Task Create_SetsTenant_And_CrossTenantGetFails()
    {
        await using var db = Db();
        var svc = Svc(db);

        var created = await svc.CreateAsync(new ItemGroupDto
        {
            GroupCode = "G-A1",
            GroupName = "Group A",
            IsActive = true
        }, TenantA, "user");

        Assert.Equal(TenantA, created.TenantId);
        Assert.NotEqual(Guid.Empty, created.Id);

        var ok = await svc.GetByIdAsync(created.Id, TenantA);
        Assert.NotNull(ok);
        Assert.Equal("G-A1", ok!.GroupCode);

        Assert.Null(await svc.GetByIdAsync(created.Id, TenantB));
        Assert.Null(await svc.GetByIdAsync(created.Id, Guid.Empty));
    }

    [Fact]
    public async Task List_FailClosed_EmptyTenant_And_ScopedByTenant()
    {
        await using var db = Db();
        db.Set<ItemGroup>().AddRange(
            new ItemGroup
            {
                Id = Guid.NewGuid(),
                TenantId = TenantA,
                GroupCode = "A-1",
                GroupName = "A One",
                IsActive = true
            },
            new ItemGroup
            {
                Id = Guid.NewGuid(),
                TenantId = TenantB,
                GroupCode = "B-1",
                GroupName = "B One",
                IsActive = true
            });
        await db.SaveChangesAsync();

        var svc = Svc(db);
        var a = (await svc.GetAllAsync(TenantA)).ToList();
        Assert.Single(a);
        Assert.Equal("A-1", a[0].GroupCode);

        Assert.Empty(await svc.GetAllAsync(Guid.Empty));
    }

    [Fact]
    public async Task Update_And_Delete_Reject_CrossTenant_And_EmptyTenant()
    {
        await using var db = Db();
        var svc = Svc(db);
        var created = await svc.CreateAsync(new ItemGroupDto
        {
            GroupCode = "G-X",
            GroupName = "Cross",
            IsActive = true
        }, TenantA, "user");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CreateAsync(new ItemGroupDto { GroupCode = "Z", GroupName = "Z" }, Guid.Empty, "user"));

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            svc.UpdateAsync(new ItemGroupDto
            {
                Id = created.Id,
                GroupCode = "G-X",
                GroupName = "Hacked"
            }, TenantB, "user"));

        Assert.False(await svc.DeleteAsync(created.Id, TenantB));
        Assert.False(await svc.DeleteAsync(created.Id, Guid.Empty));
        Assert.True(await svc.DeleteAsync(created.Id, TenantA));
    }
}
