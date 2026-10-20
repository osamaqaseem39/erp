using ERP.Application.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class InvoiceBillLinePersistenceTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Customer = Guid.NewGuid();
    private static readonly Guid Vendor = Guid.NewGuid();
    private static readonly Guid Gl = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static InvoiceService Svc(ERPDbContext db) =>
        new(new ApplicationInvoiceRepositoryAdapter(db));

    [Fact]
    public async Task InvoiceLines_Persist_And_Recalc_Header()
    {
        await using var db = Db();
        db.ChartOfAccounts.Add(new ChartOfAccount
        {
            Id = Gl,
            AccountId = Gl,
            TenantId = Tenant,
            AccountCode = "4000",
            AccountName = "Sales",
            AccountType = "Income",
            IsPostingAllowed = true,
            IsActive = true
        });
        await db.SaveChangesAsync();

        var svc = Svc(db);
        var invoiceId = await svc.AddInvoiceAsync(new InvoiceCreateDto
        {
            CustomerId = Customer,
            InvoiceNumber = "AR-LINE-1",
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(30),
            SubTotal = 0,
            TaxAmount = 0,
            TotalAmount = 0,
            Status = "Draft"
        }, Tenant, User);

        var lineId = await svc.AddInvoiceLineAsync(new InvoiceLineCreateDto
        {
            ItemId = Guid.NewGuid(),
            Description = "Widget",
            Quantity = 2,
            UnitPrice = 50,
            TaxRate = 10,
            GLAccountId = Gl
        }, invoiceId, Tenant, User);

        Assert.NotEqual(Guid.Empty, lineId);

        var lines = (await svc.GetInvoiceLinesByInvoiceIdAsync(invoiceId, Tenant)).ToList();
        Assert.Single(lines);
        Assert.Equal(110m, lines[0].TotalPrice);

        var invoice = await svc.GetInvoiceByIdAsync(invoiceId, Tenant);
        Assert.NotNull(invoice);
        Assert.Equal(100m, invoice!.SubTotal);
        Assert.Equal(10m, invoice.TaxTotal);
        Assert.Equal(110m, invoice.TotalAmount);
        Assert.Equal(110m, invoice.Balance);
        Assert.Single(invoice.InvoiceLines!);

        Assert.Equal(Guid.Empty, await svc.AddInvoiceLineAsync(new InvoiceLineCreateDto
        {
            Quantity = 1,
            UnitPrice = 10,
            GLAccountId = Guid.Empty
        }, invoiceId, Tenant, User));
    }

    [Fact]
    public async Task BillLines_Persist_And_Reject_When_Posted()
    {
        await using var db = Db();
        db.ChartOfAccounts.Add(new ChartOfAccount
        {
            Id = Gl,
            AccountId = Gl,
            TenantId = Tenant,
            AccountCode = "5000",
            AccountName = "Expense",
            AccountType = "Expense",
            IsPostingAllowed = true,
            IsActive = true
        });
        await db.SaveChangesAsync();

        var svc = Svc(db);
        var billId = await svc.AddBillAsync(new BillCreateDto
        {
            VendorId = Vendor,
            BillNumber = "AP-LINE-1",
            BillDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(15),
            Status = "Draft"
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, billId);

        var lineId = await svc.AddBillLineAsync(new BillLineCreateDto
        {
            Description = "Parts",
            Quantity = 3,
            UnitPrice = 20,
            TaxRate = 0,
            GLAccountId = Gl
        }, billId, Tenant, User);
        Assert.NotEqual(Guid.Empty, lineId);

        var bill = await svc.GetBillByIdAsync(billId, Tenant);
        Assert.Equal(60m, bill!.TotalAmount);
        Assert.Equal(60m, bill.Balance);

        Assert.Equal(billId, await svc.PostBillAsync(billId, Tenant, User));
        Assert.Equal(Guid.Empty, await svc.AddBillLineAsync(new BillLineCreateDto
        {
            Quantity = 1,
            UnitPrice = 5,
            GLAccountId = Gl
        }, billId, Tenant, User));
        Assert.False(await svc.DeleteBillAsync(billId, Tenant));
    }

    [Fact]
    public async Task FakeSuccess_Paths_Are_Gone()
    {
        await using var db = Db();
        var svc = Svc(db);
        Assert.Equal(Guid.Empty, await svc.AddInvoiceLineAsync(new InvoiceLineCreateDto
        {
            Quantity = 1,
            UnitPrice = 1,
            GLAccountId = Gl
        }, Guid.NewGuid(), Tenant, User));
        Assert.Empty(await svc.GetInvoiceLinesByInvoiceIdAsync(Guid.NewGuid(), Tenant));
        Assert.Null(await svc.GetBillByIdAsync(Guid.NewGuid(), Tenant));
        Assert.Equal(Guid.Empty, await svc.AddBillAsync(new BillCreateDto { VendorId = Guid.Empty }, Tenant, User));
    }
}
