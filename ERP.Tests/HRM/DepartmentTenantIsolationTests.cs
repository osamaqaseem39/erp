using ERP.Application.HRM;
using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.HRM;
using ERP.Shared.DTOs.HRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.HRM;

public class DepartmentTenantIsolationTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static DepartmentService Svc(ERPDbContext db) =>
        new(new DepartmentRepository(db));

    [Fact]
    public async Task Create_SetsTenant_And_CrossTenantGetFails()
    {
        await using var db = Db();
        var svc = Svc(db);

        var created = await svc.CreateAsync(new CreateDepartmentDto
        {
            Name = "Engineering",
            IsActive = true
        }, TenantA, User);

        Assert.Equal(TenantA, created.TenantId);
        Assert.NotNull(created.Id);

        Assert.NotNull(await svc.GetByIdAsync(created.Id!.Value, TenantA));
        Assert.Null(await svc.GetByIdAsync(created.Id.Value, TenantB));
        Assert.Null(await svc.GetByIdAsync(created.Id.Value, Guid.Empty));
    }

    [Fact]
    public async Task List_FailClosed_EmptyTenant_And_ScopedByTenant()
    {
        await using var db = Db();
        db.Departments.AddRange(
            new Department
            {
                Id = Guid.NewGuid(),
                TenantId = TenantA,
                Name = "Alpha",
                IsActive = true
            },
            new Department
            {
                Id = Guid.NewGuid(),
                TenantId = TenantB,
                Name = "Beta",
                IsActive = true
            });
        await db.SaveChangesAsync();

        var svc = Svc(db);
        var a = (await svc.GetAllAsync(TenantA)).ToList();
        Assert.Single(a);
        Assert.Equal("Alpha", a[0].Name);

        Assert.Empty(await svc.GetAllAsync(Guid.Empty));
        Assert.Empty(await svc.GetActiveAsync(Guid.Empty));
    }

    [Fact]
    public async Task ParentFilter_And_UpdateDelete_Reject_CrossTenant()
    {
        await using var db = Db();
        var svc = Svc(db);

        var parent = await svc.CreateAsync(new CreateDepartmentDto
        {
            Name = "Parent",
            IsActive = true
        }, TenantA, User);
        var child = await svc.CreateAsync(new CreateDepartmentDto
        {
            Name = "Child",
            ParentDepartmentId = parent.Id,
            IsActive = true
        }, TenantA, User);

        var roots = (await svc.GetAllAsync(TenantA, parentDepartmentId: Guid.Empty)).ToList();
        Assert.Contains(roots, d => d.Id == parent.Id);
        Assert.DoesNotContain(roots, d => d.Id == child.Id);

        var kids = (await svc.GetAllAsync(TenantA, parentDepartmentId: parent.Id)).ToList();
        Assert.Single(kids);
        Assert.Equal(child.Id, kids[0].Id);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CreateAsync(new CreateDepartmentDto { Name = "Z" }, Guid.Empty, User));

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            svc.UpdateAsync(new UpdateDepartmentDto
            {
                Id = parent.Id!.Value,
                Name = "Hacked"
            }, TenantB, User));

        var parentId = parent.Id!.Value;
        var childId = child.Id!.Value;
        Assert.False(await svc.DeleteAsync(parentId, TenantB));
        Assert.False(await svc.DeleteAsync(parentId, Guid.Empty));
        Assert.True(await svc.DeleteAsync(childId, TenantA));
    }
}
