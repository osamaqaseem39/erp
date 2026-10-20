using ERP.Application.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Numbering;
using ERP.Infrastructure.Repositories.CRM;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Core;
using ERP.Shared.DTOs.CRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.CRM;

public class QuoteWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid Creator = Guid.NewGuid();
    private static readonly Guid Approver = Guid.NewGuid();
    private static readonly Guid Customer = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();

    private sealed class FixedTenant : ITenantProvider
    {
        public Guid? GetTenantId() => Tenant;
    }

    private sealed class StubNumbers : IDocumentNumberService
    {
        private int _n;
        public Task<string> NextAsync(string key, CancellationToken cancellationToken = default) =>
            Task.FromResult($"Q-WF-{Interlocked.Increment(ref _n)}");
        public Task<IReadOnlyList<DocumentSequenceDto>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DocumentSequenceDto>>([]);
        public Task<DocumentSequenceDto> UpdateAsync(string key, UpdateDocumentSequenceDto dto, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static QuoteService Svc(ERPDbContext db)
    {
        var tenant = new FixedTenant();
        return new QuoteService(
            new QuoteRepository(db, tenant),
            new QuoteLineRepository(db, tenant),
            new SalesOrderRepository(db, tenant),
            new SalesOrderLineRepository(db, tenant),
            new StubNumbers());
    }

    private static async Task<Guid> CreateDraftAsync(QuoteService svc)
    {
        return await svc.CreateAsync(new CreateQuoteDto
        {
            TenantId = Tenant,
            CreatedBy = Creator.ToString(),
            CustomerId = Customer,
            QuoteDate = DateTime.UtcNow.Date,
            Status = "Accepted", // ignored — always Draft
            QuoteLines =
            [
                new CreateQuoteLineDto { ItemId = Item, Quantity = 2, UnitPrice = 25, TaxRate = 0 }
            ]
        }, Tenant, Creator);
    }

    [Fact]
    public async Task Create_Always_Draft_And_Creator_Cannot_Approve()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await CreateDraftAsync(svc);

        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.False(await svc.ApproveAsync(id, Tenant, Creator));
        Assert.True(await svc.ApproveAsync(id, Tenant, Approver));
        Assert.Equal("Accepted", (await svc.GetByIdAsync(id, Tenant))!.Status);
    }

    [Fact]
    public async Task Reject_From_Draft_Or_Sent_Blocks_Convert()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await CreateDraftAsync(svc);

        Assert.True(await svc.RejectAsync(id, Tenant, Approver));
        Assert.Equal("Rejected", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.Equal(Guid.Empty, await svc.ConvertToSalesOrderAsync(id, Tenant, Approver));
        Assert.False(await svc.SendQuoteAsync(id, Tenant, Approver));
    }

    [Fact]
    public async Task Draft_Only_Editable_After_Send()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await CreateDraftAsync(svc);

        Assert.True(await svc.UpdateAsync(new UpdateQuoteDto
        {
            Id = id,
            Notes = "ok",
            Status = "Converted",
            QuoteLines =
            [
                new CreateQuoteLineDto { ItemId = Item, Quantity = 1, UnitPrice = 10 }
            ]
        }, Tenant, Creator));
        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status);

        Assert.True(await svc.SendQuoteAsync(id, Tenant, Approver));
        Assert.False(await svc.UpdateAsync(new UpdateQuoteDto { Id = id, Notes = "nope", QuoteLines = [] }, Tenant, Approver));
        Assert.False(await svc.DeleteAsync(id, Tenant));
    }

    [Fact]
    public async Task Send_Approve_Convert_HappyPath()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await CreateDraftAsync(svc);

        Assert.Equal(Guid.Empty, await svc.ConvertToSalesOrderAsync(id, Tenant, Approver)); // not accepted
        Assert.True(await svc.SendQuoteAsync(id, Tenant, Approver));
        Assert.Equal("Sent", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.NotNull((await svc.GetByIdAsync(id, Tenant))!.SentDate);

        Assert.True(await svc.ApproveAsync(id, Tenant, Approver));
        var soId = await svc.ConvertToSalesOrderAsync(id, Tenant, Approver);
        Assert.NotEqual(Guid.Empty, soId);
        Assert.Equal("Converted", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.False(await svc.RejectAsync(id, Tenant, Approver));
    }

    [Fact]
    public async Task Tenant_Mismatch_Blocks_Workflow()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await CreateDraftAsync(svc);
        Assert.False(await svc.ApproveAsync(id, Guid.NewGuid(), Approver));
        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status);
    }
}
