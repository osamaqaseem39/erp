using ERP.Api.GraphQL.Finance;
using ERP.Application.Finance;
using ERP.Domain.CRM;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class GraphQLSendInvoiceTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid CustomerId = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public void Mutations_ExposeSendInvoice()
    {
        var names = typeof(FinanceMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(FinanceMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        Assert.Contains("SendInvoice", names);
    }

    [Fact]
    public async Task GraphQL_SendInvoice_PromotesDraft_ToIssued()
    {
        await using var db = Db();
        var notifier = new CapturingInvoiceDeliveryNotifier();
        var invoiceId = Guid.NewGuid();
        db.Customers.Add(new Customer
        {
            Id = CustomerId,
            TenantId = Tenant,
            CustomerCode = "C-GQL",
            CustomerName = "Acme",
            CompanyName = "Acme",
            Email = "billing@acme.test"
        });
        db.Invoices.Add(new Invoice
        {
            Id = invoiceId,
            InvoiceId = invoiceId,
            TenantId = Tenant,
            CustomerId = CustomerId,
            InvoiceNumber = "INV-GQL-SEND",
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(14),
            TotalAmount = 100,
            Balance = 100,
            Status = "Draft",
            CurrencyCode = "USD"
        });
        await db.SaveChangesAsync();

        var invoices = new InvoiceService(new ApplicationInvoiceRepositoryAdapter(db), notifier);
        var gql = new FinanceMutations(null!, invoices, null!, null!, null!, null!);

        Assert.True(await gql.SendInvoice(invoiceId, Tenant, User));
        Assert.Equal("Issued", (await db.Invoices.SingleAsync()).Status);
        Assert.Equal("billing@acme.test", notifier.Last!.RecipientEmail);
    }
}
