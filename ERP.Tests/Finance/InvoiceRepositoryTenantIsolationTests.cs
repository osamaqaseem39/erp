using ERP.Application.Finance;
using ERP.Domain.CRM;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class InvoiceRepositoryTenantIsolationTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();
    private static readonly Guid CustomerA = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static ApplicationInvoiceRepositoryAdapter Repo(ERPDbContext db) => new(db);

    [Fact]
    public async Task Invoice_Get_List_Lines_FailClosed_And_Isolate()
    {
        await using var db = Db();
        var invoiceId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        db.Invoices.Add(new Invoice
        {
            Id = invoiceId,
            InvoiceId = invoiceId,
            TenantId = TenantA,
            CustomerId = CustomerA,
            InvoiceNumber = "INV-A1",
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(30),
            Status = "Issued",
            TotalAmount = 100m,
            Balance = 100m,
            IsActive = true
        });
        db.InvoiceLines.Add(new InvoiceLine
        {
            InvoiceLineId = lineId,
            InvoiceId = invoiceId,
            Quantity = 1,
            UnitPrice = 100m,
            TaxRate = 0,
            LineTotal = 100m
        });
        await db.SaveChangesAsync();

        var repo = Repo(db);

        Assert.NotNull(await repo.GetByIdAsync(invoiceId, TenantA));
        Assert.Null(await repo.GetByIdAsync(invoiceId, TenantB));
        Assert.Null(await repo.GetByIdAsync(invoiceId, Guid.Empty));

        Assert.Single(await repo.GetAllAsync(TenantA));
        Assert.Empty(await repo.GetAllAsync(TenantB));
        Assert.Empty(await repo.GetAllAsync(Guid.Empty));
        Assert.Equal(0, await repo.GetCountAsync(Guid.Empty));

        Assert.Single(await repo.GetLinesByInvoiceIdAsync(invoiceId, TenantA));
        Assert.Empty(await repo.GetLinesByInvoiceIdAsync(invoiceId, TenantB));
        Assert.Empty(await repo.GetLinesByInvoiceIdAsync(invoiceId, Guid.Empty));
        Assert.Null(await repo.GetLineByIdAsync(lineId, Guid.Empty));
        Assert.Null(await repo.GetLineByIdAsync(lineId, TenantB));
    }

    [Fact]
    public async Task Bill_And_CustomerEmail_FailClosed()
    {
        await using var db = Db();
        var billId = Guid.NewGuid();
        db.Bills.Add(new Bill
        {
            Id = billId,
            BillId = billId,
            TenantId = TenantA,
            BillNumber = "BILL-A1",
            BillDate = DateTime.UtcNow.Date,
            Status = "Issued",
            TotalAmount = 50m,
            Balance = 50m,
            IsActive = true
        });
        db.Customers.Add(new Customer
        {
            Id = CustomerA,
            TenantId = TenantA,
            CustomerCode = "C1",
            CustomerName = "Acme",
            Email = "ar@acme.test",
            IsActive = true
        });
        await db.SaveChangesAsync();

        var repo = Repo(db);

        Assert.NotNull(await repo.GetBillByIdAsync(billId, TenantA));
        Assert.Null(await repo.GetBillByIdAsync(billId, TenantB));
        Assert.Null(await repo.GetBillByIdAsync(billId, Guid.Empty));
        Assert.Empty(await repo.GetAllBillsAsync(Guid.Empty));
        Assert.Equal(0, await repo.GetBillCountAsync(Guid.Empty));

        Assert.Equal("ar@acme.test", await repo.GetCustomerEmailAsync(CustomerA, TenantA));
        Assert.Null(await repo.GetCustomerEmailAsync(CustomerA, TenantB));
        Assert.Null(await repo.GetCustomerEmailAsync(CustomerA, Guid.Empty));
    }

    [Fact]
    public async Task Add_Rejects_Empty_Tenant_And_Aging_FailClosed()
    {
        await using var db = Db();
        var repo = Repo(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.AddAsync(new Invoice
            {
                InvoiceId = Guid.NewGuid(),
                TenantId = Guid.Empty,
                InvoiceNumber = "X",
                InvoiceDate = DateTime.UtcNow.Date,
                DueDate = DateTime.UtcNow.Date,
                Status = "Draft"
            }));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.AddBillAsync(new Bill
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.Empty,
                BillNumber = "X",
                BillDate = DateTime.UtcNow.Date,
                Status = "Draft"
            }));

        Assert.Empty(await repo.GetAgedReceivablesAsync(Guid.Empty, DateTime.UtcNow, []));
        Assert.Empty(await repo.GetAgedPayablesAsync(Guid.Empty, DateTime.UtcNow, []));
    }
}
