using ERP.Application.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Numbering;
using ERP.Infrastructure.Repositories.CRM;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Core;
using ERP.Shared.DTOs.CRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.CRM;

public class LeadWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid OtherTenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private sealed class FixedTenant : ITenantProvider
    {
        public Guid? GetTenantId() => Tenant;
    }

    private sealed class StubNumbers : IDocumentNumberService
    {
        private int _n;
        public Task<string> NextAsync(string key, CancellationToken cancellationToken = default) =>
            Task.FromResult($"LD-WF-{Interlocked.Increment(ref _n)}");
        public Task<IReadOnlyList<DocumentSequenceDto>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DocumentSequenceDto>>([]);
        public Task<DocumentSequenceDto> UpdateAsync(string key, UpdateDocumentSequenceDto dto, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static LeadService Svc(ERPDbContext db)
    {
        var tenant = new FixedTenant();
        return new LeadService(
            new LeadRepository(db, tenant),
            new CustomerRepository(db, tenant),
            new OpportunityRepository(db, tenant),
            new StubNumbers());
    }

    private static async Task<Guid> CreateNewAsync(LeadService svc) =>
        await svc.CreateAsync(new CreateLeadDto
        {
            TenantId = Tenant,
            CreatedBy = User.ToString(),
            CompanyName = "Acme",
            Email = "a@acme.test",
            Status = "Converted" // ignored — always New
        }, Tenant, User);

    [Fact]
    public async Task Create_Issues_Server_Code_And_Persists_Fields()
    {
        await using var db = Db();
        var svc = Svc(db);
        var owner = Guid.NewGuid();

        var id = await svc.CreateAsync(new CreateLeadDto
        {
            TenantId = Tenant,
            LeadCode = "HACKED-1", // ignored — the sequence issues the code
            FirstName = " Sara ",
            LastName = "Khan",
            Email = "Sara@Acme.Test",
            LeadSource = "Referral",
            Priority = "High",
            EstimatedValue = 5000,
            AssignedToUserId = owner
        }, Tenant, User);

        var lead = await svc.GetByIdAsync(id, Tenant);
        Assert.NotNull(lead);
        Assert.Equal("LD-WF-1", lead!.LeadCode);
        Assert.Equal("Sara", lead.FirstName);
        Assert.Equal("Khan", lead.LastName);
        Assert.Equal("sara@acme.test", lead.Email);
        Assert.Equal("Referral", lead.LeadSource);
        Assert.Equal("Referral", lead.Source);
        Assert.Equal("High", lead.Priority);
        Assert.Equal(5000, lead.EstimatedValue);
        Assert.Equal(owner, lead.AssignedToUserId);

        await svc.UpdateAsync(new UpdateLeadDto { Id = id, LeadCode = "CHANGED", FirstName = "Sara", Email = "sara@acme.test" }, Tenant, User);
        Assert.Equal("LD-WF-1", (await svc.GetByIdAsync(id, Tenant))!.LeadCode);
    }

    [Fact]
    public async Task Create_Blocks_Duplicate_Email_Or_Phone_Unless_Allowed()
    {
        await using var db = Db();
        var svc = Svc(db);
        await svc.CreateAsync(new CreateLeadDto { FirstName = "A", Email = "dup@acme.test", Phone = "+92 300 1234567" }, Tenant, User);

        await Assert.ThrowsAsync<ERP.Shared.Exceptions.ConflictException>(() =>
            svc.CreateAsync(new CreateLeadDto { FirstName = "B", Email = "DUP@acme.test" }, Tenant, User));
        await Assert.ThrowsAsync<ERP.Shared.Exceptions.ConflictException>(() =>
            svc.CreateAsync(new CreateLeadDto { FirstName = "C", Email = "c@acme.test", Phone = "0300-1234567" }, Tenant, User));

        var id = await svc.CreateAsync(new CreateLeadDto { FirstName = "D", Email = "dup@acme.test", AllowDuplicate = true }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id);

        // Other tenants never count as duplicates.
        Assert.Empty(await svc.FindDuplicatesAsync("dup@acme.test", null, null, OtherTenant));
    }

    [Fact]
    public async Task Score_Is_Calculated_And_Activity_Updates_Last_Contact()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await svc.CreateAsync(new CreateLeadDto
        {
            FirstName = "S", Email = "s@acme.test", Phone = "03001112223", CompanyName = "Acme",
            LeadSource = "Referral", Priority = "High", EstimatedValue = 20_000, LeadScore = 99 // ignored
        }, Tenant, User);
        // 10+10+10 (email, phone, company) + 20 referral + 15 high + 10 value = 75
        Assert.Equal(75, (await svc.GetByIdAsync(id, Tenant))!.LeadScore);

        var activities = new ActivityService(new ActivityRepository(db, new FixedTenant()), new LeadRepository(db, new FixedTenant()));
        var contactedAt = DateTime.UtcNow.AddDays(-1);
        await activities.CreateAsync(new CreateActivityDto { Subject = "Call", ActivityType = "Call", LeadId = id, StartDate = contactedAt }, Tenant, User);

        var lead = (await svc.GetByIdAsync(id, Tenant))!;
        Assert.Equal(contactedAt, lead.LastContactDate);
        Assert.Equal(85, lead.LeadScore);
    }

    [Fact]
    public async Task Convert_To_Opportunity_Uses_Supplied_Details()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await CreateNewAsync(svc);
        Assert.True(await svc.QualifyAsync(id, Tenant, User));
        var close = DateTime.UtcNow.Date.AddDays(30);
        var oppId = await svc.ConvertToOpportunityAsync(id, Tenant, User,
            new ConvertLeadToOpportunityDto { OpportunityName = "Acme rollout", Amount = 12_000, Probability = 40, ExpectedCloseDate = close });

        var opp = await db.Opportunities.FindAsync(oppId);
        Assert.Equal("Acme rollout", opp!.OpportunityName);
        Assert.Equal(12_000, opp.Amount);
        Assert.Equal(40, opp.Probability);
        Assert.Equal(close, opp.ExpectedCloseDate);
    }

    [Fact]
    public async Task Create_Always_New_And_Convert_Requires_Qualified()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await CreateNewAsync(svc);

        Assert.Equal("New", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.Equal(Guid.Empty, await svc.ConvertToOpportunityAsync(id, Tenant, User));

        Assert.True(await svc.QualifyAsync(id, Tenant, User));
        Assert.Equal("Qualified", (await svc.GetByIdAsync(id, Tenant))!.Status);

        var oppId = await svc.ConvertToOpportunityAsync(id, Tenant, User);
        Assert.NotEqual(Guid.Empty, oppId);
        Assert.Equal("Converted", (await svc.GetByIdAsync(id, Tenant))!.Status);
    }

    [Fact]
    public async Task Editable_Until_Lost_And_Status_Not_Via_Update()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await CreateNewAsync(svc);

        Assert.True(await svc.UpdateAsync(new UpdateLeadDto
        {
            Id = id,
            CompanyName = "Acme2",
            Status = "Qualified"
        }, Tenant, User));
        Assert.Equal("New", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.Equal("Acme2", (await svc.GetByIdAsync(id, Tenant))!.CompanyName);

        Assert.False(await svc.MarkLostAsync(id, new LoseLeadDto(), Tenant, User)); // reason required
        Assert.True(await svc.MarkLostAsync(id, new LoseLeadDto { LostReason = "Price", LostNote = "Too expensive" }, Tenant, User));
        var lost = (await svc.GetByIdAsync(id, Tenant))!;
        Assert.Equal("Lost", lost.Status);
        Assert.Equal("Price", lost.LostReason);
        Assert.Equal("Too expensive", lost.LostNote);
        Assert.False(await svc.UpdateAsync(new UpdateLeadDto { Id = id, CompanyName = "Nope" }, Tenant, User));
        Assert.False(await svc.QualifyAsync(id, Tenant, User));
        Assert.False(await svc.DeleteAsync(id, Tenant));
    }

    [Fact]
    public async Task Convert_To_Customer_And_Tenant_Guard()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await CreateNewAsync(svc);

        Assert.False(await svc.QualifyAsync(id, OtherTenant, User));
        Assert.True(await svc.QualifyAsync(id, Tenant, User));

        var customerId = await svc.ConvertToCustomerAsync(id, Tenant, User);
        Assert.NotEqual(Guid.Empty, customerId);
        Assert.Equal("Converted", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.Equal(Guid.Empty, await svc.ConvertToOpportunityAsync(id, Tenant, User));
    }
}
