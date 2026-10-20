using ERP.Application.Services.Procurement;
using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Procurement;
using ERP.Shared.DTOs.Procurement;
using ERP.Shared.Mappers.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Procurement;

public class VendorApproveSuspendTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Company = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static VendorService Svc(ERPDbContext db) =>
        new(new VendorRepository(db), new VendorBankAccountRepository(db), new VendorMapper());

    [Fact]
    public async Task Suspend_Then_Approve_TogglesIsActive()
    {
        await using var db = Db();
        var id = Guid.NewGuid();
        db.Vendors.Add(new Vendor
        {
            Id = id,
            TenantId = Tenant,
            CompanyId = Company,
            VendorCode = "V-1",
            VendorName = "Acme",
            IsActive = true
        });
        await db.SaveChangesAsync();

        var svc = Svc(db);
        Assert.True(await svc.SuspendAsync(id, new SuspendVendorDto { Reason = "credit hold" }, Tenant, User));
        var suspended = await db.Vendors.SingleAsync(v => v.Id == id);
        Assert.False(suspended.IsActive);

        Assert.True(await svc.ApproveAsync(id, Tenant, User));
        var active = await db.Vendors.SingleAsync(v => v.Id == id);
        Assert.True(active.IsActive);
    }

    [Fact]
    public async Task Approve_FailClosed_EmptyTenant()
    {
        await using var db = Db();
        var id = Guid.NewGuid();
        db.Vendors.Add(new Vendor
        {
            Id = id,
            TenantId = Tenant,
            CompanyId = Company,
            VendorCode = "V-2",
            VendorName = "Beta",
            IsActive = false
        });
        await db.SaveChangesAsync();

        var svc = Svc(db);
        Assert.False(await svc.ApproveAsync(id, Guid.Empty, User));
        Assert.False(await svc.SuspendAsync(id, new SuspendVendorDto(), Guid.Empty, User));
    }
}
