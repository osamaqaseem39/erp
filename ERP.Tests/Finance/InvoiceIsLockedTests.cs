using ERP.Application.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class InvoiceIsLockedTests
{
    private static readonly Guid Tenant = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task IsLockedAsync_ReflectsFlagAndImmutableStatus()
    {
        await using var db = Db();
        var openId = Guid.NewGuid();
        var lockedId = Guid.NewGuid();
        var postedId = Guid.NewGuid();

        db.Invoices.AddRange(
            new Invoice
            {
                Id = openId,
                InvoiceId = openId,
                TenantId = Tenant,
                InvoiceNumber = "AR-1",
                Status = "Draft",
                IsLocked = false,
                InvoiceDate = DateTime.UtcNow.Date,
                DueDate = DateTime.UtcNow.Date.AddDays(30)
            },
            new Invoice
            {
                Id = lockedId,
                InvoiceId = lockedId,
                TenantId = Tenant,
                InvoiceNumber = "AR-2",
                Status = "Issued",
                IsLocked = true,
                InvoiceDate = DateTime.UtcNow.Date,
                DueDate = DateTime.UtcNow.Date.AddDays(30)
            },
            new Invoice
            {
                Id = postedId,
                InvoiceId = postedId,
                TenantId = Tenant,
                InvoiceNumber = "AR-3",
                Status = "Posted",
                IsLocked = false,
                InvoiceDate = DateTime.UtcNow.Date,
                DueDate = DateTime.UtcNow.Date.AddDays(30)
            });
        await db.SaveChangesAsync();

        var repo = new ApplicationInvoiceRepositoryAdapter(db);
        Assert.False(await repo.IsLockedAsync(openId, Tenant));
        Assert.True(await repo.IsLockedAsync(lockedId, Tenant));
        Assert.True(await repo.IsLockedAsync(postedId, Tenant));
        Assert.False(await repo.IsLockedAsync(openId, Guid.Empty));
    }
}
