using ERP.Application.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Numbering;
using ERP.Infrastructure.Repositories.CRM;
using ERP.Shared.DTOs.Core;
using ERP.Shared.DTOs.CRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Crm;

public class PackagingEnquiryWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid Creator = Guid.NewGuid();
    private static readonly Guid Approver = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private sealed class FakeNumbers : IDocumentNumberService
    {
        private int _n;
        public Task<string> NextAsync(string key, CancellationToken cancellationToken = default) =>
            Task.FromResult($"{key}-{++_n:D4}");
        public Task<IReadOnlyList<DocumentSequenceDto>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DocumentSequenceDto>>(Array.Empty<DocumentSequenceDto>());
        public Task<DocumentSequenceDto> UpdateAsync(string key, UpdateDocumentSequenceDto dto, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    [Fact]
    public async Task Enquiry_Approve_Reject_UpdateKeepsDraft()
    {
        await using var db = Db();
        var svc = new SaleEnquiryService(new SaleEnquiryRepository(db), new FakeNumbers());

        var id = await svc.CreateAsync(new CreateSaleEnquiryDto
        {
            Notes = "pack job",
            Lines = [new CreateSaleEnquiryLineDto { ItemDescription = "Pouch", Quantity = 1000 }]
        }, Tenant, Creator);

        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status);

        Assert.True(await svc.UpdateAsync(new UpdateSaleEnquiryDto
        {
            Id = id,
            Notes = "still draft",
            Lines = [new CreateSaleEnquiryLineDto { ItemDescription = "Pouch", Quantity = 1000 }]
        }, Tenant, Creator));
        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status);

        // FIX-6: creator cannot approve own doc
        Assert.False(await svc.ApproveAsync(id, Tenant, Creator));
        Assert.True(await svc.ApproveAsync(id, Tenant, Approver));
        Assert.Equal("Approved", (await svc.GetByIdAsync(id, Tenant))!.Status);

        var id2 = await svc.CreateAsync(new CreateSaleEnquiryDto
        {
            Lines = [new CreateSaleEnquiryLineDto { ItemDescription = "Bag", Quantity = 10 }]
        }, Tenant, Creator);
        Assert.True(await svc.RejectAsync(id2, Tenant, Approver));
        Assert.Equal("Rejected", (await svc.GetByIdAsync(id2, Tenant))!.Status);
    }
}
