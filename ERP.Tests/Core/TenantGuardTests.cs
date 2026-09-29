using ERP.Domain.Core;
using ERP.Domain.CRM;
using ERP.Infrastructure.Auditing;
using ERP.Infrastructure.Database;
using ERP.Shared.Abstractions;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Core;

public class TenantGuardTests
{
    private static readonly Guid Mine = Guid.NewGuid();
    private static readonly Guid Theirs = Guid.NewGuid();
    private readonly string _dbName = Guid.NewGuid().ToString();

    private ERPDbContext Db(Guid? tenant, bool superAdmin = false) => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(_dbName)
        .AddInterceptors(new TenantGuardInterceptor(new Request(tenant, superAdmin)))
        .Options);

    private static Customer NewCustomer(Guid tenant) => new() { TenantId = tenant, CustomerCode = "C-1", CustomerName = "Acme", CompanyName = "Acme" };

    private async Task<Guid> SeedTheirCustomerAsync()
    {
        await using var db = Db(tenant: null);
        var customer = NewCustomer(Theirs);
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        return customer.Id;
    }

    [Fact]
    public async Task NewRowsWithoutATenantGetTheCallersTenant()
    {
        await using var db = Db(Mine);
        var customer = NewCustomer(Guid.Empty);
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        Assert.Equal(Mine, customer.TenantId);
    }

    [Fact]
    public async Task CannotInsertIntoAnotherTenant_EvenIfTheRequestBodySaysSo()
    {
        await using var db = Db(Mine);
        db.Customers.Add(NewCustomer(Theirs));

        await Assert.ThrowsAsync<ForbiddenException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task CannotUpdateOrDeleteAnotherTenantsRows()
    {
        var id = await SeedTheirCustomerAsync();

        await using (var db = Db(Mine))
        {
            (await db.Customers.SingleAsync(c => c.Id == id)).CustomerName = "Hijacked";
            await Assert.ThrowsAsync<ForbiddenException>(() => db.SaveChangesAsync());
        }
        await using (var db = Db(Mine))
        {
            db.Customers.Remove(await db.Customers.SingleAsync(c => c.Id == id));
            await Assert.ThrowsAsync<ForbiddenException>(() => db.SaveChangesAsync());
        }
    }

    [Fact]
    public async Task CannotMoveAnotherTenantsRowIntoYourOwn()
    {
        var id = await SeedTheirCustomerAsync();
        await using var db = Db(Mine);
        (await db.Customers.SingleAsync(c => c.Id == id)).TenantId = Mine;

        await Assert.ThrowsAsync<ForbiddenException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task SuperAdminMayWorkAcrossTenants()
    {
        await using var db = Db(Mine, superAdmin: true);
        db.Customers.Add(NewCustomer(Theirs));

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task RequestsWithoutATenantAreNotAffected()
    {
        await using var db = Db(tenant: null);
        db.Customers.Add(NewCustomer(Theirs));
        db.Tenants.Add(new Tenant { TenantName = "New", Domain = "new.test", DatabaseName = "", ConnectionString = "" });

        await db.SaveChangesAsync();
    }

    private sealed class Request(Guid? tenant, bool superAdmin) : IAuditContext
    {
        public Guid? UserId => Guid.NewGuid();
        public Guid? TenantId => tenant;
        public string? IpAddress => null;
        public string? UserAgent => null;
        public bool IsSuperAdmin => superAdmin;
    }
}
