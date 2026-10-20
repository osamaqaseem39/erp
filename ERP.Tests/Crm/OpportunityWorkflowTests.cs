using ERP.Application.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Numbering;
using ERP.Infrastructure.Repositories.CRM;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Core;
using ERP.Shared.DTOs.CRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.CRM;

public class OpportunityWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid OtherTenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Customer = Guid.NewGuid();

    private sealed class FixedTenant : ITenantProvider
    {
        public Guid? GetTenantId() => Tenant;
    }

    private sealed class StubNumbers : IDocumentNumberService
    {
        private int _n;
        public Task<string> NextAsync(string key, CancellationToken cancellationToken = default) =>
            Task.FromResult($"OP-WF-{Interlocked.Increment(ref _n)}");
        public Task<IReadOnlyList<DocumentSequenceDto>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DocumentSequenceDto>>([]);
        public Task<DocumentSequenceDto> UpdateAsync(string key, UpdateDocumentSequenceDto dto, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static OpportunityService Svc(ERPDbContext db)
    {
        var tenant = new FixedTenant();
        return new OpportunityService(
            new OpportunityRepository(db, tenant),
            new QuoteRepository(db, tenant),
            new StubNumbers());
    }

    private static async Task<Guid> CreateOpenAsync(OpportunityService svc)
    {
        return await svc.CreateAsync(new CreateOpportunityDto
        {
            TenantId = Tenant,
            CreatedBy = User.ToString(),
            CustomerId = Customer,
            Title = "Deal",
            Amount = 1000,
            Status = "Won" // ignored — always Open
        }, Tenant, User);
    }

    [Fact]
    public async Task Create_Always_Open_And_Ignores_Dto_Status()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await CreateOpenAsync(svc);

        var dto = await svc.GetByIdAsync(id, Tenant);
        Assert.NotNull(dto);
        Assert.Equal("Open", dto!.Status);
        Assert.False(string.IsNullOrWhiteSpace(dto.OpportunityCode));
    }

    [Fact]
    public async Task Convert_Requires_Won_And_Blocks_Open()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await CreateOpenAsync(svc);

        Assert.Equal(Guid.Empty, await svc.ConvertToQuoteAsync(id, Tenant, User));
        Assert.True(await svc.MarkWonAsync(id, Tenant, User));
        Assert.Equal("Won", (await svc.GetByIdAsync(id, Tenant))!.Status);

        var quoteId = await svc.ConvertToQuoteAsync(id, Tenant, User);
        Assert.NotEqual(Guid.Empty, quoteId);
        Assert.Equal("Converted", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.False(await svc.MarkWonAsync(id, Tenant, User));
    }

    [Fact]
    public async Task Open_Only_Editable_And_Lost_Blocks_Convert()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await CreateOpenAsync(svc);

        Assert.True(await svc.UpdateAsync(new UpdateOpportunityDto
        {
            Id = id,
            Title = "Updated",
            Amount = 2000,
            Status = "Won"
        }, Tenant, User));
        Assert.Equal("Open", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.Equal("Updated", (await svc.GetByIdAsync(id, Tenant))!.Title);

        Assert.True(await svc.MarkLostAsync(id, Tenant, User, "no budget"));
        Assert.Equal("Lost", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.False(await svc.UpdateAsync(new UpdateOpportunityDto { Id = id, Title = "Nope" }, Tenant, User));
        Assert.Equal(Guid.Empty, await svc.ConvertToQuoteAsync(id, Tenant, User));
    }

    [Fact]
    public async Task Tenant_Mismatch_And_Cancel_Are_Guarded()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await CreateOpenAsync(svc);

        Assert.False(await svc.MarkWonAsync(id, OtherTenant, User));
        Assert.Equal("Open", (await svc.GetByIdAsync(id, Tenant))!.Status);

        Assert.True(await svc.CancelAsync(id, Tenant, User));
        Assert.Equal("Cancelled", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.False(await svc.DeleteAsync(id, Tenant));
    }
}
