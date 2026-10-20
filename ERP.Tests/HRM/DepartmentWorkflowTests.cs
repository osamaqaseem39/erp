using ERP.Application.HRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.HRM;
using ERP.Shared.DTOs.HRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.HRM;

public class DepartmentWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Create_Update_List_SoftDelete()
    {
        await using var db = Db();
        var svc = new DepartmentService(new DepartmentRepository(db));

        var created = await svc.CreateAsync(new CreateDepartmentDto
        {
            Name = "Engineering",
            Description = "Product eng",
            IsActive = true
        }, Tenant, User);
        Assert.NotNull(created.Id);
        Assert.Equal("Engineering", created.Name);

        var child = await svc.CreateAsync(new CreateDepartmentDto
        {
            Name = "Platform",
            ParentDepartmentId = created.Id,
            IsActive = true
        }, Tenant, User);
        Assert.Equal(created.Id, child.ParentDepartmentId);

        var all = (await svc.GetAllAsync(Tenant)).ToList();
        Assert.Equal(2, all.Count);

        var updated = await svc.UpdateAsync(new UpdateDepartmentDto
        {
            Id = created.Id!.Value,
            Name = "Engineering & Platform",
            IsActive = true
        }, Tenant, User);
        Assert.Equal("Engineering & Platform", updated.Name);

        Assert.True(await svc.DeleteAsync(created.Id!.Value, Tenant));
        var after = await svc.GetByIdAsync(created.Id!.Value, Tenant);
        Assert.False(after!.IsActive);

        var active = (await svc.GetActiveAsync(Tenant)).ToList();
        Assert.DoesNotContain(active, d => d.Id == created.Id);
        Assert.Contains(active, d => d.Id == child.Id);
    }
}
