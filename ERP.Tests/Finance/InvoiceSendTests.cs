using ERP.Application.Finance;
using ERP.Domain.CRM;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class InvoiceSendTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid CustomerId = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Send_Draft_WithCustomerEmail_Notifies_And_Promotes_To_Issued()
    {
        await using var db = Db();
        var notifier = new CapturingInvoiceDeliveryNotifier();
        var invoiceId = Guid.NewGuid();
        db.Customers.Add(new Customer
        {
            Id = CustomerId,
            TenantId = Tenant,
            CustomerCode = "C-1",
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
            InvoiceNumber = "INV-SEND-1",
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(14),
            TotalAmount = 250,
            Balance = 250,
            Status = "Draft",
            CurrencyCode = "USD"
        });
        await db.SaveChangesAsync();

        var svc = new InvoiceService(new ApplicationInvoiceRepositoryAdapter(db), notifier);
        Assert.True(await svc.SendInvoiceAsync(invoiceId, Tenant, User));

        Assert.NotNull(notifier.Last);
        Assert.Equal("billing@acme.test", notifier.Last!.RecipientEmail);
        Assert.Equal("INV-SEND-1", notifier.Last.InvoiceNumber);
        Assert.Equal(250m, notifier.Last.TotalAmount);

        var invoice = await db.Invoices.SingleAsync(i => i.InvoiceId == invoiceId);
        Assert.Equal("Issued", invoice.Status);
    }

    [Fact]
    public async Task Send_Issued_Requeues_Without_Status_Change()
    {
        await using var db = Db();
        var notifier = new CapturingInvoiceDeliveryNotifier();
        var invoiceId = Guid.NewGuid();
        db.Customers.Add(new Customer
        {
            Id = CustomerId,
            TenantId = Tenant,
            CustomerCode = "C-1",
            CustomerName = "Acme",
            CompanyName = "Acme",
            Email = "ap@acme.test"
        });
        db.Invoices.Add(new Invoice
        {
            Id = invoiceId,
            InvoiceId = invoiceId,
            TenantId = Tenant,
            CustomerId = CustomerId,
            InvoiceNumber = "INV-SEND-2",
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(7),
            TotalAmount = 40,
            Balance = 40,
            Status = "Issued",
            CurrencyCode = "PKR"
        });
        await db.SaveChangesAsync();

        var svc = new InvoiceService(new ApplicationInvoiceRepositoryAdapter(db), notifier);
        Assert.True(await svc.SendInvoiceAsync(invoiceId, Tenant, User));
        Assert.Equal("Issued", (await db.Invoices.SingleAsync()).Status);
        Assert.Equal("ap@acme.test", notifier.Last!.RecipientEmail);
    }

    [Fact]
    public async Task Send_Fails_Without_Email_Or_When_Cancelled()
    {
        await using var db = Db();
        var notifier = new CapturingInvoiceDeliveryNotifier();
        var noEmailId = Guid.NewGuid();
        var cancelledId = Guid.NewGuid();
        var custNoEmail = Guid.NewGuid();
        var custWithEmail = Guid.NewGuid();
        db.Customers.AddRange(
            new Customer
            {
                Id = custNoEmail, TenantId = Tenant, CustomerCode = "C-0",
                CustomerName = "Silent", CompanyName = "Silent"
            },
            new Customer
            {
                Id = custWithEmail, TenantId = Tenant, CustomerCode = "C-2",
                CustomerName = "Gone", CompanyName = "Gone", Email = "gone@x.test"
            });
        db.Invoices.AddRange(
            new Invoice
            {
                Id = noEmailId, InvoiceId = noEmailId, TenantId = Tenant,
                CustomerId = custNoEmail, InvoiceNumber = "INV-NE",
                InvoiceDate = DateTime.UtcNow.Date, DueDate = DateTime.UtcNow.Date,
                TotalAmount = 10, Balance = 10, Status = "Draft", CurrencyCode = "USD"
            },
            new Invoice
            {
                Id = cancelledId, InvoiceId = cancelledId, TenantId = Tenant,
                CustomerId = custWithEmail, InvoiceNumber = "INV-CX",
                InvoiceDate = DateTime.UtcNow.Date, DueDate = DateTime.UtcNow.Date,
                TotalAmount = 10, Balance = 0, Status = "Cancelled", CurrencyCode = "USD"
            });
        await db.SaveChangesAsync();

        var svc = new InvoiceService(new ApplicationInvoiceRepositoryAdapter(db), notifier);
        Assert.False(await svc.SendInvoiceAsync(noEmailId, Tenant, User));
        Assert.False(await svc.SendInvoiceAsync(cancelledId, Tenant, User));
        Assert.Null(notifier.Last);
    }
}
