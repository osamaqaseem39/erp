using ERP.Application.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Procurement;
using ERP.Shared.DTOs.Procurement;
using Microsoft.EntityFrameworkCore;
using RFQDetailRepository = ERP.Infrastructure.Repositories.Procurement.RFQDetailRepository;
using RFQVendorResponseRepository = ERP.Infrastructure.Repositories.Procurement.RFQVendorResponseRepository;

namespace ERP.Tests.Procurement;

public class RfqWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid ItemA = Guid.NewGuid();
    private static readonly Guid Vendor = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static RequestForQuotationService Svc(ERPDbContext db) => new(
        new RequestForQuotationRepository(db),
        new RFQDetailRepository(db),
        new RFQVendorResponseRepository(db));

    [Fact]
    public async Task Create_Send_Respond_Award_Close()
    {
        await using var db = Db();
        var svc = Svc(db);

        var id = await svc.CreateAsync(new CreateRequestForQuotationDto
        {
            Title = "Office supplies",
            DueDate = DateTime.UtcNow.AddDays(10),
            Details =
            [
                new CreateRFQDetailDto { ItemId = ItemA, Quantity = 5, Notes = "A4 paper" }
            ]
        }, Tenant, User);

        var draft = await svc.GetByIdAsync(id, Tenant);
        Assert.Equal("Draft", draft!.Status);
        Assert.Single(draft.RFQDetails!);

        Assert.True(await svc.SendAsync(id, Tenant, User));
        Assert.Equal("Open", (await svc.GetByIdAsync(id, Tenant))!.Status);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.UpdateAsync(new UpdateRequestForQuotationDto
            {
                Id = id,
                Title = "Nope"
            }, Tenant, User));

        var responseId = await svc.SubmitVendorResponseAsync(new CreateRFQVendorResponseDto
        {
            RFQId = id,
            VendorId = Vendor,
            TotalAmount = 250m
        }, Tenant, User);

        Assert.True(await svc.AwardAsync(id, responseId, Tenant, User));
        var awarded = await svc.GetByIdAsync(id, Tenant);
        Assert.Equal("Awarded", awarded!.Status);
        Assert.Equal(250m, awarded.EstimatedValue);
        Assert.Contains(awarded.VendorResponses!, r => r.Status == "Accepted");

        Assert.True(await svc.CloseAsync(id, Tenant, User));
        Assert.Equal("Closed", (await svc.GetByIdAsync(id, Tenant))!.Status);
    }

    [Fact]
    public async Task Delete_Only_Draft()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await svc.CreateAsync(new CreateRequestForQuotationDto
        {
            Title = "Temp",
            Details = [new CreateRFQDetailDto { ItemId = ItemA, Quantity = 1 }]
        }, Tenant, User);

        Assert.True(await svc.SendAsync(id, Tenant, User));
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.DeleteAsync(id, Tenant));
    }
}
