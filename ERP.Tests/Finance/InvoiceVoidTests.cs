using ERP.Application.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class InvoiceVoidTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Void_Cancels_Unpaid_Invoice_And_Blocks_When_Paid()
    {
        await using var db = Db();
        var svc = new InvoiceService(new ApplicationInvoiceRepositoryAdapter(db));
        var unpaidId = Guid.NewGuid();
        var paidId = Guid.NewGuid();
        db.Invoices.AddRange(
            new Invoice
            {
                Id = unpaidId, InvoiceId = unpaidId, TenantId = Tenant,
                InvoiceNumber = "INV-U", InvoiceDate = DateTime.UtcNow.Date,
                DueDate = DateTime.UtcNow.Date.AddDays(30),
                TotalAmount = 100, Balance = 100, Status = "Issued", CurrencyCode = "USD"
            },
            new Invoice
            {
                Id = paidId, InvoiceId = paidId, TenantId = Tenant,
                InvoiceNumber = "INV-P", InvoiceDate = DateTime.UtcNow.Date,
                DueDate = DateTime.UtcNow.Date.AddDays(30),
                TotalAmount = 80, Balance = 30, Status = "PartiallyPaid", CurrencyCode = "USD"
            });
        await db.SaveChangesAsync();

        Assert.True(await svc.VoidInvoiceAsync(unpaidId, new VoidInvoiceDto { Reason = "Error" }, Tenant, User));
        var unpaid = await db.Invoices.FirstAsync(i => i.InvoiceId == unpaidId);
        Assert.Equal("Cancelled", unpaid.Status);
        Assert.Equal(0m, unpaid.Balance);
        Assert.True(unpaid.IsLocked);

        Assert.False(await svc.VoidInvoiceAsync(paidId, new VoidInvoiceDto { Reason = "Nope" }, Tenant, User));
        Assert.False(await svc.VoidInvoiceAsync(unpaidId, new VoidInvoiceDto { Reason = "again" }, Tenant, User));
    }
}
