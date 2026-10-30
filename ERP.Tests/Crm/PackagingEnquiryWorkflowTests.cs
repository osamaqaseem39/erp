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

        var customer = Guid.NewGuid();
        var id = await svc.CreateAsync(new CreateSaleEnquiryDto
        {
            CustomerId = customer,
            Notes = "pack job",
            Lines = [new CreateSaleEnquiryLineDto { ItemDescription = "Pouch", Quantity = 1000 }]
        }, Tenant, Creator);

        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status);

        Assert.True(await svc.UpdateAsync(new UpdateSaleEnquiryDto
        {
            Id = id,
            CustomerId = customer,
            Notes = "still draft",
            Lines = [new CreateSaleEnquiryLineDto { ItemDescription = "Pouch", Quantity = 1000 }]
        }, Tenant, Creator));
        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status);

        // FIX-6: creator cannot approve own doc — now a clear 403 instead of a silent false/404
        await Assert.ThrowsAsync<ERP.Shared.Exceptions.ForbiddenException>(() => svc.ApproveAsync(id, Tenant, Creator));
        Assert.True(await svc.ApproveAsync(id, Tenant, Approver));
        Assert.Equal("Approved", (await svc.GetByIdAsync(id, Tenant))!.Status);

        var id2 = await svc.CreateAsync(new CreateSaleEnquiryDto
        {
            Lines = [new CreateSaleEnquiryLineDto { ItemDescription = "Bag", Quantity = 10 }]
        }, Tenant, Creator);
        Assert.True(await svc.RejectAsync(id2, Tenant, Approver, "Wrong spec"));
        var rejected = (await svc.GetByIdAsync(id2, Tenant))!;
        Assert.Equal("Rejected", rejected.Status);
        Assert.Equal("Wrong spec", rejected.RejectionReason);

        // Rejected can be reopened to Draft, fixed and resubmitted.
        Assert.True(await svc.ReopenAsync(id2, Tenant, Creator));
        Assert.Equal("Draft", (await svc.GetByIdAsync(id2, Tenant))!.Status);
        await Assert.ThrowsAsync<ERP.Shared.Exceptions.ConflictException>(() => svc.ReopenAsync(id, Tenant, Creator)); // Approved
    }

    [Fact]
    public async Task Enquiry_Rejects_Bad_Lines_And_Unready_Approval()
    {
        await using var db = Db();
        var svc = new SaleEnquiryService(new SaleEnquiryRepository(db), new FakeNumbers());

        await Assert.ThrowsAsync<ERP.Shared.Exceptions.ValidationException>(() => svc.CreateAsync(new CreateSaleEnquiryDto
        {
            Lines = [new CreateSaleEnquiryLineDto { ItemDescription = "", Quantity = 0 }]
        }, Tenant, Creator));

        // No customer and no lines: saved as draft, but cannot be approved.
        var id = await svc.CreateAsync(new CreateSaleEnquiryDto(), Tenant, Creator);
        var ex = await Assert.ThrowsAsync<ERP.Shared.Exceptions.ValidationException>(() => svc.ApproveAsync(id, Tenant, Approver));
        Assert.Contains("ready", ex.Message);
    }

    [Fact]
    public async Task Approved_Enquiry_Becomes_PreCost_With_Copied_Lines_Once()
    {
        await using var db = Db();
        var enquiries = new SaleEnquiryRepository(db);
        var preCostRepo = new SalePreCostRepository(db);
        var svc = new SaleEnquiryService(enquiries, new FakeNumbers(), preCostRepo);
        var preCosts = new SalePreCostService(preCostRepo, new FakeNumbers(), enquiries);

        var customer = Guid.NewGuid();
        var id = await svc.CreateAsync(new CreateSaleEnquiryDto
        {
            CustomerId = customer,
            Lines =
            [
                new CreateSaleEnquiryLineDto { ItemDescription = "Pouch", Quantity = 1000, Size = "10x12", PET = 12, Ups = 4 },
                new CreateSaleEnquiryLineDto { ItemDescription = "Bag", Quantity = 500 }
            ]
        }, Tenant, Creator);

        // Not approved yet.
        await Assert.ThrowsAsync<ERP.Shared.Exceptions.ConflictException>(() => preCosts.CreateFromEnquiryAsync(id, Tenant, Approver));

        Assert.True(await svc.ApproveAsync(id, Tenant, Approver));
        var preCostId = await preCosts.CreateFromEnquiryAsync(id, Tenant, Approver);

        var pc = (await preCosts.GetByIdAsync(preCostId, Tenant))!;
        Assert.Equal("Draft", pc.Status);
        Assert.Equal(id, pc.SaleEnquiryId);
        Assert.Equal(customer, pc.CustomerId);
        Assert.Equal(2, pc.Lines.Count);
        var pouch = pc.Lines.Single(l => l.ItemDescription == "Pouch");
        Assert.Equal(1000, pouch.Quantity);
        Assert.Equal("10x12", pouch.Size);
        Assert.Equal(12, pouch.PET);
        Assert.Equal(4, pouch.Ups);

        // A second pre-cost for the same enquiry is refused, and the enquiry links to the first.
        await Assert.ThrowsAsync<ERP.Shared.Exceptions.ConflictException>(() => preCosts.CreateFromEnquiryAsync(id, Tenant, Approver));
        var enquiry = (await svc.GetByIdAsync(id, Tenant))!;
        Assert.Equal(preCostId, enquiry.SalePreCostId);
        Assert.Equal(2, enquiry.LineCount);
        Assert.Equal(1500, enquiry.TotalQuantity);
    }
}
