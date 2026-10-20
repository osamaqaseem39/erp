using ERP.Application.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.CRM;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.CRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.CRM;

public class ActivityWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid OtherTenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private sealed class FixedTenant : ITenantProvider
    {
        public Guid? GetTenantId() => Tenant;
    }

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static ActivityService Svc(ERPDbContext db) =>
        new(new ActivityRepository(db, new FixedTenant()));

    private static async Task<Guid> CreateOpenAsync(ActivityService svc) =>
        await svc.CreateAsync(new CreateActivityDto
        {
            TenantId = Tenant,
            CreatedBy = User.ToString(),
            ActivityType = "Call",
            Subject = "Follow up",
            StartDate = DateTime.UtcNow.Date,
            AssignedToUserId = User,
            Status = "Completed" // ignored — always Open
        }, Tenant, User);

    [Fact]
    public async Task Create_Always_Open_And_Complete_Blocks_Reschedule()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await CreateOpenAsync(svc);

        Assert.Equal("Open", (await svc.GetByIdAsync(id, Tenant))!.Status);
        var when = DateTime.UtcNow.Date.AddDays(3);
        Assert.True(await svc.RescheduleAsync(id, when, Tenant, User));
        Assert.Equal(when, (await svc.GetByIdAsync(id, Tenant))!.StartDate);

        Assert.True(await svc.CompleteAsync(id, Tenant, User));
        Assert.Equal("Completed", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.False(await svc.RescheduleAsync(id, when.AddDays(1), Tenant, User));
        Assert.False(await svc.CancelAsync(id, Tenant, User));
        Assert.False(await svc.DeleteAsync(id, Tenant));
    }

    [Fact]
    public async Task Open_Only_Editable_And_Cancel_Is_Terminal()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await CreateOpenAsync(svc);

        Assert.True(await svc.UpdateAsync(new UpdateActivityDto
        {
            Id = id,
            ActivityType = "Meeting",
            Subject = "Demo",
            StartDate = DateTime.UtcNow.Date,
            AssignedToUserId = User,
            Status = "Completed"
        }, Tenant, User));
        Assert.Equal("Open", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.Equal("Demo", (await svc.GetByIdAsync(id, Tenant))!.Subject);

        Assert.True(await svc.CancelAsync(id, Tenant, User));
        Assert.Equal("Cancelled", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.False(await svc.UpdateAsync(new UpdateActivityDto
        {
            Id = id,
            ActivityType = "Call",
            Subject = "Nope",
            StartDate = DateTime.UtcNow.Date,
            AssignedToUserId = User
        }, Tenant, User));
        Assert.False(await svc.CompleteAsync(id, Tenant, User));
    }

    [Fact]
    public async Task Tenant_Mismatch_Blocks_Workflow()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await CreateOpenAsync(svc);

        Assert.False(await svc.CompleteAsync(id, OtherTenant, User));
        Assert.Equal("Open", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.True(await svc.DeleteAsync(id, Tenant));
    }
}
