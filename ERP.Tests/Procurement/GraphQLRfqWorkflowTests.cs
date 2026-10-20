using ERP.Api.GraphQL.Procurement;
using ERP.Application.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Procurement;
using ERP.Shared.DTOs.Procurement;
using Microsoft.EntityFrameworkCore;
using RFQDetailRepository = ERP.Infrastructure.Repositories.Procurement.RFQDetailRepository;
using RFQVendorResponseRepository = ERP.Infrastructure.Repositories.Procurement.RFQVendorResponseRepository;

namespace ERP.Tests.Procurement;

public class GraphQLRfqWorkflowTests
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
    public void Surface_ExposesRfqQueriesAndMutations()
    {
        var queries = typeof(ProcurementQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(ProcurementQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        var mutations = typeof(ProcurementMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(ProcurementMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetRequestForQuotation", queries);
        Assert.Contains("GetRequestForQuotations", queries);
        Assert.Contains("CreateRequestForQuotation", mutations);
        Assert.Contains("SendRequestForQuotation", mutations);
        Assert.Contains("CancelRequestForQuotation", mutations);
        Assert.Contains("CloseRequestForQuotation", mutations);
        Assert.Contains("SubmitRfqVendorResponse", mutations);
        Assert.Contains("AwardRequestForQuotation", mutations);
        Assert.Contains("DeleteRequestForQuotation", mutations);
    }

    [Fact]
    public async Task GraphQL_Create_Send_Respond_Award_Close()
    {
        await using var db = Db();
        var svc = Svc(db);
        var queries = new ProcurementQueries();
        var mutations = new ProcurementMutations();

        var id = await mutations.CreateRequestForQuotation(
            svc,
            new CreateRequestForQuotationDto
            {
                Title = "GQL Office supplies",
                DueDate = DateTime.UtcNow.AddDays(10),
                Details = [new CreateRFQDetailDto { ItemId = ItemA, Quantity = 5 }]
            },
            Tenant,
            User);
        Assert.NotEqual(Guid.Empty, id);

        var draft = await queries.GetRequestForQuotation(svc, id, Tenant);
        Assert.Equal("Draft", draft!.Status);

        Assert.True(await mutations.SendRequestForQuotation(svc, id, Tenant, User));
        Assert.Equal("Open", (await queries.GetRequestForQuotation(svc, id, Tenant))!.Status);

        var responseId = await mutations.SubmitRfqVendorResponse(
            svc,
            new CreateRFQVendorResponseDto
            {
                RFQId = id,
                VendorId = Vendor,
                TotalAmount = 199m
            },
            Tenant,
            User);

        Assert.True(await mutations.AwardRequestForQuotation(svc, id, responseId, Tenant, User));
        Assert.Equal("Awarded", (await queries.GetRequestForQuotation(svc, id, Tenant))!.Status);

        Assert.True(await mutations.CloseRequestForQuotation(svc, id, Tenant, User));
        Assert.Equal("Closed", (await queries.GetRequestForQuotation(svc, id, Tenant))!.Status);

        var listed = (await queries.GetRequestForQuotations(svc, Tenant, status: "Closed")).ToList();
        Assert.Contains(listed, r => r.Id == id);
    }
}
