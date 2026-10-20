using ERP.Application.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Adapters;
using ERP.Infrastructure.Repositories.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class Wave2PaymentApplyTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Customer = Guid.NewGuid();
    private static readonly Guid Vendor = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static PaymentService Payments(ERPDbContext db) =>
        new(new PaymentRepositoryAdapter(new PaymentRepository(db)), new ApplicationInvoiceRepositoryAdapter(db), db);

    [Fact]
    public async Task ApplyToInvoice_ReducesBalance_AndMarksPaid()
    {
        await using var db = Db();
        var invoices = new InvoiceService(new ApplicationInvoiceRepositoryAdapter(db));
        var invoiceId = await invoices.AddInvoiceAsync(new InvoiceCreateDto
        {
            CustomerId = Customer,
            InvoiceNumber = "AR-PAY-1",
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(30),
            SubTotal = 100,
            TaxAmount = 0,
            TotalAmount = 100,
            Status = "Issued"
        }, Tenant, User);

        var payments = Payments(db);
        Assert.True(await payments.ApplyPaymentToInvoiceAsync(new PaymentApplicationDto
        {
            InvoiceId = invoiceId,
            Amount = 40
        }, Tenant, User));

        var afterPartial = await invoices.GetInvoiceByIdAsync(invoiceId, Tenant);
        Assert.NotNull(afterPartial);
        Assert.Equal(60m, afterPartial!.Balance);
        Assert.Equal("PartiallyPaid", afterPartial.Status);

        Assert.True(await payments.ApplyPaymentToInvoiceAsync(new PaymentApplicationDto
        {
            InvoiceId = invoiceId,
            Amount = 60
        }, Tenant, User));

        var afterPaid = await invoices.GetInvoiceByIdAsync(invoiceId, Tenant);
        Assert.Equal(0m, afterPaid!.Balance);
        Assert.Equal("Paid", afterPaid.Status);
        Assert.False(await payments.ApplyPaymentToInvoiceAsync(new PaymentApplicationDto
        {
            InvoiceId = invoiceId,
            Amount = 1
        }, Tenant, User));
    }

    [Fact]
    public async Task ApplyToBill_ReducesBalance_AndMarksPaid()
    {
        await using var db = Db();
        var bills = new BillService(new BillRepository(db));
        var created = await bills.CreateAsync(new CreateBillDto
        {
            VendorId = Vendor,
            BillNumber = "AP-PAY-1",
            BillDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(15),
            SubTotal = 200,
            TaxAmount = 0,
            TotalAmount = 200,
            Status = "Issued"
        }, Tenant, User);

        var payments = Payments(db);
        Assert.True(await payments.ApplyPaymentToBillAsync(new PaymentApplicationDto
        {
            BillId = created.Id,
            Amount = 50
        }, Tenant, User));

        var partial = await bills.GetByIdAsync(created.Id, Tenant);
        Assert.Equal(150m, (await db.Bills.FirstAsync(b => b.Id == created.Id)).Balance);
        Assert.Equal("PartiallyPaid", partial!.Status);

        Assert.True(await payments.ApplyPaymentToBillAsync(new PaymentApplicationDto
        {
            BillId = created.Id,
            Amount = 150
        }, Tenant, User));

        var paid = await bills.GetByIdAsync(created.Id, Tenant);
        Assert.Equal(0m, (await db.Bills.FirstAsync(b => b.Id == created.Id)).Balance);
        Assert.Equal("Paid", paid!.Status);
    }

    [Fact]
    public async Task MarkAsPaid_CreatesPayment_AndUpdatesBill()
    {
        await using var db = Db();
        var payments = Payments(db);
        var bills = new BillService(new BillRepository(db), payments);
        var created = await bills.CreateAsync(new CreateBillDto
        {
            VendorId = Vendor,
            BillNumber = "AP-MARK-1",
            BillDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(15),
            SubTotal = 80,
            TaxAmount = 0,
            TotalAmount = 80,
            Status = "Issued"
        }, Tenant, User);

        var result = await bills.MarkAsPaidAsync(created.Id, new PaymentDto
        {
            Amount = 30,
            PaymentMethod = "BankTransfer",
            Notes = "partial"
        }, Tenant, User);

        Assert.Equal(50m, result.Balance);
        Assert.Equal("PartiallyPaid", result.Status);
        Assert.Single(await db.Payments.Where(p => p.TenantId == Tenant && p.RelatedBillId == created.Id).ToListAsync());

        result = await bills.MarkAsPaidAsync(created.Id, new PaymentDto { Amount = 50 }, Tenant, User);
        Assert.Equal(0m, result.Balance);
        Assert.Equal("Paid", result.Status);
        Assert.Equal(2, await db.Payments.CountAsync(p => p.TenantId == Tenant && p.RelatedBillId == created.Id));
    }

    [Fact]
    public async Task CreatePayment_WithRelatedInvoice_AppliesAutomatically()
    {
        await using var db = Db();
        var invoices = new InvoiceService(new ApplicationInvoiceRepositoryAdapter(db));
        var invoiceId = await invoices.AddInvoiceAsync(new InvoiceCreateDto
        {
            CustomerId = Customer,
            InvoiceNumber = "AR-PAY-2",
            TotalAmount = 75,
            SubTotal = 75,
            TaxAmount = 0,
            Status = "Issued"
        }, Tenant, User);

        var payments = Payments(db);
        var payId = await payments.CreateAsync(new CreatePaymentDto
        {
            TenantId = Tenant,
            CreatedBy = User.ToString(),
            Amount = 75,
            RelatedInvoiceId = invoiceId,
            PaymentMethod = "Cash"
        });

        Assert.NotEqual(Guid.Empty, payId);
        var invoice = await invoices.GetInvoiceByIdAsync(invoiceId, Tenant);
        Assert.Equal(0m, invoice!.Balance);
        Assert.Equal("Paid", invoice.Status);
        var payment = await payments.GetByIdAsync(payId);
        Assert.Equal("Completed", payment!.Status);
        Assert.Equal(invoiceId, payment.RelatedInvoiceId);
    }

    [Fact]
    public async Task Void_Restores_Invoice_Balance()
    {
        await using var db = Db();
        var invoices = new InvoiceService(new ApplicationInvoiceRepositoryAdapter(db));
        var invoiceId = await invoices.AddInvoiceAsync(new InvoiceCreateDto
        {
            CustomerId = Customer,
            InvoiceNumber = "AR-VOID-1",
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(30),
            SubTotal = 100,
            TaxAmount = 0,
            TotalAmount = 100,
            Status = "Issued"
        }, Tenant, User);

        var payments = Payments(db);
        var payId = await payments.CreateAsync(new CreatePaymentDto
        {
            TenantId = Tenant,
            CreatedBy = User.ToString(),
            Amount = 100,
            RelatedInvoiceId = invoiceId,
            PaymentMethod = "Cash"
        });
        Assert.Equal(0m, (await invoices.GetInvoiceByIdAsync(invoiceId, Tenant))!.Balance);

        Assert.True(await payments.VoidAsync(payId, new VoidPaymentDto { Reason = "Duplicate" }, Tenant, User));
        var voided = await payments.GetByIdAsync(payId);
        Assert.Equal("Voided", voided!.Status);
        Assert.False(voided.IsActive);
        Assert.Contains("Duplicate", voided.Notes);

        var invoice = await invoices.GetInvoiceByIdAsync(invoiceId, Tenant);
        Assert.Equal(100m, invoice!.Balance);
        Assert.Equal("Issued", invoice.Status);

        Assert.False(await payments.VoidAsync(payId, new VoidPaymentDto { Reason = "again" }, Tenant, User));
    }

    [Fact]
    public async Task Refund_Restores_Invoice_Balance()
    {
        await using var db = Db();
        var invoices = new InvoiceService(new ApplicationInvoiceRepositoryAdapter(db));
        var invoiceId = await invoices.AddInvoiceAsync(new InvoiceCreateDto
        {
            CustomerId = Customer,
            InvoiceNumber = "AR-REF-1",
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(30),
            SubTotal = 50,
            TaxAmount = 0,
            TotalAmount = 50,
            Status = "Issued"
        }, Tenant, User);

        var payments = Payments(db);
        var payId = await payments.CreateAsync(new CreatePaymentDto
        {
            TenantId = Tenant,
            CreatedBy = User.ToString(),
            Amount = 50,
            RelatedInvoiceId = invoiceId,
            PaymentMethod = "Cash"
        });
        Assert.Equal(0m, (await invoices.GetInvoiceByIdAsync(invoiceId, Tenant))!.Balance);

        Assert.True(await payments.RefundAsync(payId, new RefundDto { RefundAmount = 50, Reason = "Return" }, Tenant, User));
        var refunded = await payments.GetByIdAsync(payId);
        Assert.Equal("Refunded", refunded!.Status);
        Assert.Equal(0m, refunded.Amount);

        var invoice = await invoices.GetInvoiceByIdAsync(invoiceId, Tenant);
        Assert.Equal(50m, invoice!.Balance);
        Assert.Equal("Issued", invoice.Status);
    }
}
