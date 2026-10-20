using ERP.Application.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Adapters;
using ERP.Infrastructure.Repositories.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class PaymentTenantIsolationTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Repo_Get_List_Invoice_FailClosed_And_Isolate()
    {
        await using var db = Db();
        var paymentId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        db.Payments.Add(new Payment
        {
            Id = paymentId,
            TenantId = TenantA,
            PaymentNumber = "PAY-A1",
            PaymentDate = DateTime.UtcNow.Date,
            PaymentType = "Cash",
            PaymentMethod = "Cash",
            Amount = 25m,
            RelatedInvoiceId = invoiceId,
            Status = "Completed"
        });
        await db.SaveChangesAsync();

        var repo = new PaymentRepository(db);

        Assert.NotNull(await repo.GetByIdAsync(paymentId, TenantA));
        Assert.Null(await repo.GetByIdAsync(paymentId, TenantB));
        Assert.Null(await repo.GetByIdAsync(paymentId, Guid.Empty));

        Assert.Single(await repo.GetAllAsync(TenantA));
        Assert.Empty(await repo.GetAllAsync(TenantB));
        Assert.Empty(await repo.GetAllAsync(Guid.Empty));

        Assert.Single(await repo.GetByInvoiceAsync(invoiceId, TenantA));
        Assert.Empty(await repo.GetByInvoiceAsync(invoiceId, TenantB));
        Assert.Empty(await repo.GetByInvoiceAsync(invoiceId, Guid.Empty));

        Assert.Equal(Guid.Empty, await repo.CreateAsync(new Payment
        {
            TenantId = Guid.Empty,
            PaymentNumber = "X",
            PaymentDate = DateTime.UtcNow.Date,
            Amount = 1m
        }));
    }

    [Fact]
    public async Task Service_Reads_FailClosed_On_Empty_Tenant()
    {
        await using var db = Db();
        var adapter = new PaymentRepositoryAdapter(new PaymentRepository(db));
        var svc = new PaymentService(adapter, new ApplicationInvoiceRepositoryAdapter(db));

        Assert.Null(await svc.GetByIdAsync(Guid.NewGuid(), Guid.Empty));
        Assert.Empty(await svc.GetAllAsync(Guid.Empty));
        Assert.Empty(await svc.GetByInvoiceAsync(Guid.NewGuid(), Guid.Empty));
        Assert.False(await svc.DeleteAsync(Guid.NewGuid(), Guid.Empty));
        Assert.False(await svc.ProcessPaymentAsync(Guid.NewGuid(), Guid.Empty, Guid.NewGuid()));
    }
}
