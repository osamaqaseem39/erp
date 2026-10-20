using ERP.Api.GraphQL.Sales;
using ERP.Application.Finance;
using ERP.Application.Sales;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Sales;
using ERP.Shared.DTOs.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Sales;

public class GraphQLSalesCreditNoteWiringTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Customer = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static SalesCreditNoteService Credit(ERPDbContext db) =>
        new(
            new SalesCreditNoteRepository(db),
            new SalesCreditNoteLineRepository(db),
            new SalesInvoiceRepository(db),
            new InvoiceService(new ApplicationInvoiceRepositoryAdapter(db)));

    private static SalesInvoiceService Sales(ERPDbContext db) =>
        new(
            new SalesInvoiceRepository(db),
            new SalesInvoiceLineRepository(db),
            new InvoiceService(new ApplicationInvoiceRepositoryAdapter(db)));

    [Fact]
    public async Task GraphQL_Create_List_Post_Void_CreditNote()
    {
        await using var db = Db();
        var sales = Sales(db);
        var credit = Credit(db);
        var queries = new SalesQueries();
        var mutations = new SalesMutations();

        var siId = await sales.CreateAsync(new CreateSalesInvoiceDto
        {
            InvoiceNumber = "SI-GQL-CN",
            CustomerId = Customer,
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(30),
            Status = "Draft",
            Lines =
            [
                new CreateSalesInvoiceLineDto
                {
                    ItemId = Item,
                    Description = "Widget",
                    Quantity = 1,
                    UnitPrice = 100,
                    TaxRate = 0,
                    LineNumber = 1
                }
            ]
        }, Tenant, User);
        Assert.True(await sales.PostInvoiceAsync(siId, Tenant, User));

        var created = await mutations.AddSalesCreditNote(
            credit,
            new CreateSalesCreditNoteDto
            {
                CreditNoteNumber = "CN-GQL-1",
                CreditNoteDate = DateTime.UtcNow.Date,
                CustomerId = Customer,
                OriginalSalesInvoiceId = siId,
                Lines =
                [
                    new CreateSalesCreditNoteLineDto
                    {
                        ItemId = Item,
                        Description = "Return",
                        Quantity = 1,
                        UnitPrice = 100,
                        TaxRate = 0,
                        LineNumber = 1
                    }
                ]
            },
            Tenant,
            User);
        Assert.True(created.Id.HasValue && created.Id.Value != Guid.Empty);
        var cnId = created.Id!.Value;
        Assert.Equal("Draft", created.Status);

        var listed = (await queries.GetSalesCreditNotes(credit, Tenant, customerId: Customer)).ToList();
        Assert.Contains(listed, n => n.Id == cnId);

        Assert.True(await mutations.PostSalesCreditNote(credit, cnId, Tenant, User));
        var posted = await queries.GetSalesCreditNote(credit, cnId, Tenant);
        Assert.Equal("Posted", posted!.Status);

        Assert.True(await mutations.VoidSalesCreditNote(
            credit, cnId, new VoidSalesCreditNoteDto { Reason = "test" }, Tenant, User));
        var voided = await queries.GetSalesCreditNote(credit, cnId, Tenant);
        Assert.Equal("Voided", voided!.Status);
    }
}
