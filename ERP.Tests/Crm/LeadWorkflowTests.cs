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

        Assert.True(await svc.MarkLostAsync(id, Tenant, User));
        Assert.Equal("Lost", (await svc.GetByIdAsync(id, Tenant))!.Status);
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
