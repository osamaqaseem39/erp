using ERP.Application.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Numbering;
using ERP.Infrastructure.Repositories.CRM;
using ERP.Shared.DTOs.Core;
using ERP.Shared.DTOs.CRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Crm;

public class PackagingPreCostWorkflowTests
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
            Task.FromResult<IReadOnlyList<DocumentSequenceDto>>([]);
        public Task<DocumentSequenceDto> UpdateAsync(string key, UpdateDocumentSequenceDto dto, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    [Fact]
    public async Task PreCost_Approve_Reject_UpdateKeepsDraft()
    {
        await using var db = Db();
        var svc = new SalePreCostService(new SalePreCostRepository(db), new FakeNumbers());

        var id = await svc.CreateAsync(new CreateSalePreCostDto
        {
            Notes = "cost build",
            Lines = [new CreateSalePreCostLineDto { ItemDescription = "Pouch", Quantity = 500, MaterialCost = 10, UnitRate = 12 }]
        }, Tenant, Creator);

        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status);

        Assert.True(await svc.UpdateAsync(new UpdateSalePreCostDto
        {
            Id = id,
            Notes = "still draft",
            Lines = [new CreateSalePreCostLineDto { ItemDescription = "Pouch", Quantity = 500, MaterialCost = 10, UnitRate = 12 }]
        }, Tenant, Creator));
        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status);

        Assert.False(await svc.ApproveAsync(id, Tenant, Creator));
        Assert.True(await svc.ApproveAsync(id, Tenant, Approver));
        Assert.Equal("Approved", (await svc.GetByIdAsync(id, Tenant))!.Status);

        var id2 = await svc.CreateAsync(new CreateSalePreCostDto
        {
            Lines = [new CreateSalePreCostLineDto { ItemDescription = "Bag", Quantity = 1 }]
        }, Tenant, Creator);
        Assert.True(await svc.RejectAsync(id2, Tenant, Approver));
        Assert.Equal("Rejected", (await svc.GetByIdAsync(id2, Tenant))!.Status);
    }
}
