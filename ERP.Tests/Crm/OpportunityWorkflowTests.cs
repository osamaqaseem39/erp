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
    public async Task Create_Issues_Server_Code_And_Saves_Value_Stage_Probability()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await svc.CreateAsync(new CreateOpportunityDto
        {
            OpportunityCode = "HACKED", // ignored
            OpportunityName = " Acme rollout ",
            CustomerId = Customer,
            EstimatedValue = 20_000, // the old form field still lands in Amount
            Stage = "proposal",
            Probability = null,
            Currency = "eur",
            ExpectedCloseDate = DateTime.UtcNow.Date.AddDays(10),
            AssignedTo = User
        }, Tenant, User);

        var dto = (await svc.GetByIdAsync(id, Tenant))!;
        Assert.Equal("OP-WF-1", dto.OpportunityCode);
        Assert.Equal("Acme rollout", dto.OpportunityName);
        Assert.Equal(20_000, dto.Amount);
        Assert.Equal("Proposal", dto.Stage);
        Assert.Equal(50, dto.Probability);          // stage default
        Assert.Equal(10_000, dto.WeightedValue);    // 20k x 50%
        Assert.Equal("EUR", dto.Currency);
        Assert.Equal(User, dto.AssignedToUserId);

        // Editing cannot change the code or jump to a closed stage.
        await svc.UpdateAsync(new UpdateOpportunityDto
        {
            Id = id, OpportunityCode = "CHANGED", OpportunityName = "Acme rollout", CustomerId = Customer,
            Stage = "Closed Won", Amount = 20_000, ExpectedCloseDate = dto.ExpectedCloseDate
        }, Tenant, User);
        dto = (await svc.GetByIdAsync(id, Tenant))!;
        Assert.Equal("OP-WF-1", dto.OpportunityCode);
        Assert.Equal("Open", dto.Status);
        Assert.NotEqual("Closed Won", dto.Stage);
    }

    [Fact]
    public async Task Stage_Change_Win_And_Lose_Set_Probability_And_Reason()
    {
        await using var db = Db();
        var svc = Svc(db);

        var id = await CreateOpenAsync(svc);
        Assert.False(await svc.UpdateStageAsync(id, "Closed Won", 100, Tenant));
        Assert.True(await svc.UpdateStageAsync(id, "Negotiation", 80, Tenant));
        Assert.Equal(80, (await svc.GetByIdAsync(id, Tenant))!.Probability);

        Assert.True(await svc.MarkWonAsync(id, Tenant, User));
        var won = (await svc.GetByIdAsync(id, Tenant))!;
        Assert.Equal(100, won.Probability);
        Assert.Equal(1000, won.ActualValue);

        var lostId = await CreateOpenAsync(svc);
        Assert.True(await svc.MarkLostAsync(lostId, Tenant, User, " Price "));
        var lost = (await svc.GetByIdAsync(lostId, Tenant))!;
        Assert.Equal(0, lost.Probability);
        Assert.Equal("Price", lost.CloseReason);
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
