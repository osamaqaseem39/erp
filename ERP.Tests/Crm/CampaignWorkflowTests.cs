using ERP.Application.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.CRM;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.CRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.CRM;

public class CampaignWorkflowTests
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

    private static CampaignService Svc(ERPDbContext db) =>
        new(new CampaignRepository(db, new FixedTenant()));

    private static async Task<Guid> CreateDraftAsync(CampaignService svc) =>
        await svc.CreateAsync(new CreateCampaignDto
        {
            TenantId = Tenant,
            CreatedBy = User.ToString(),
            CampaignName = "Spring Push",
            CampaignType = "Email",
            Status = "Active", // ignored — always Draft
            StartDate = DateTime.UtcNow.Date
        }, Tenant, User);

    [Fact]
    public async Task Create_Always_Draft_And_Launch_Pause_Complete()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await CreateDraftAsync(svc);

        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.False(await svc.PauseAsync(id, Tenant, User));
        Assert.False(await svc.CompleteAsync(id, Tenant, User));

        Assert.True(await svc.LaunchAsync(id, Tenant, User));
        Assert.Equal("Active", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.NotNull((await svc.GetByIdAsync(id, Tenant))!.LaunchDate);

        Assert.True(await svc.PauseAsync(id, Tenant, User));
        Assert.Equal("Paused", (await svc.GetByIdAsync(id, Tenant))!.Status);

        Assert.True(await svc.LaunchAsync(id, Tenant, User));
        Assert.True(await svc.CompleteAsync(id, Tenant, User));
        Assert.Equal("Completed", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.False(await svc.LaunchAsync(id, Tenant, User));
        Assert.False(await svc.DeleteAsync(id, Tenant));
    }

    [Fact]
    public async Task Draft_Only_Editable_And_Cancel_Blocks_Launch()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await CreateDraftAsync(svc);

        Assert.True(await svc.UpdateAsync(new UpdateCampaignDto
        {
            Id = id,
            CampaignName = "Renamed",
            Status = "Completed"
        }, Tenant, User));
        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.Equal("Renamed", (await svc.GetByIdAsync(id, Tenant))!.CampaignName);

        Assert.True(await svc.CancelAsync(id, Tenant, User));
        Assert.Equal("Cancelled", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.False(await svc.UpdateAsync(new UpdateCampaignDto { Id = id, CampaignName = "Nope" }, Tenant, User));
        Assert.False(await svc.LaunchAsync(id, Tenant, User));
    }

    [Fact]
    public async Task Tenant_Mismatch_Blocks_Workflow()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await CreateDraftAsync(svc);

        Assert.False(await svc.LaunchAsync(id, OtherTenant, User));
        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.True(await svc.DeleteAsync(id, Tenant));
    }
}
