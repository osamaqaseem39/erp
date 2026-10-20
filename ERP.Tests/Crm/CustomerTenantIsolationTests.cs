using ERP.Application.CRM;
using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.CRM;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.CRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Crm;

public class CustomerTenantIsolationTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static CustomerService Svc(ERPDbContext db) =>
        new(new CustomerRepository(db, new FixedTenant(TenantA)));

    [Fact]
    public async Task Create_Stamps_Tenant_And_Forces_Active()
    {
        await using var db = Db();
        var svc = Svc(db);

        var id = await svc.CreateAsync(new CreateCustomerDto
        {
            CustomerCode = "C-A1",
            CustomerName = "Alpha",
            CompanyName = "Alpha Co",
            Status = "Inactive"
        }, TenantA, User);

        Assert.NotEqual(Guid.Empty, id);
        var row = await db.Customers.SingleAsync(c => c.Id == id);
        Assert.Equal(TenantA, row.TenantId);
        Assert.Equal("Active", row.Status);
        Assert.True(row.IsActive);

        var dto = await svc.GetByIdAsync(id, TenantA);
        Assert.Equal("Active", dto!.Status);
    }

    [Fact]
    public async Task Cross_Tenant_And_Empty_Fail_Closed()
    {
        await using var db = Db();
        var id = Guid.NewGuid();
        db.Customers.Add(new Customer
        {
            Id = id,
            TenantId = TenantB,
            CustomerCode = "B-1",
            CustomerName = "Secret",
            CompanyName = "Secret",
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var svc = Svc(db);
        Assert.Null(await svc.GetByIdAsync(id, TenantA));
        Assert.Empty(await svc.GetAllAsync(TenantA));
        Assert.Empty(await svc.GetAllAsync(Guid.Empty));
        Assert.False(await svc.UpdateAsync(new UpdateCustomerDto
        {
            Id = id,
            CompanyName = "Hijack",
            CustomerName = "Hijack",
            Status = "Inactive"
        }, TenantA, User));
        Assert.False(await svc.DeleteAsync(id, TenantA));
        Assert.False(await svc.ActivateAsync(id, TenantA, User));
        Assert.False(await svc.DeactivateAsync(id, TenantA, User));
        Assert.False(await svc.ActivateAsync(id, Guid.Empty, User));
    }

    [Fact]
    public async Task Update_Ignores_Status_Activate_Deactivate_Workflow()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await svc.CreateAsync(new CreateCustomerDto
        {
            CustomerCode = "C-W1",
            CustomerName = "Flow",
            CompanyName = "Flow Co"
        }, TenantA, User);

        Assert.True(await svc.UpdateAsync(new UpdateCustomerDto
        {
            Id = id,
            CompanyName = "Flow Co",
            CustomerName = "Flow Renamed",
            Status = "Suspended"
        }, TenantA, User));

        var mid = await svc.GetByIdAsync(id, TenantA);
        Assert.Equal("Active", mid!.Status);
        Assert.Equal("Flow Renamed", mid.CustomerName);

        Assert.True(await svc.DeactivateAsync(id, TenantA, User));
        var off = await svc.GetByIdAsync(id, TenantA);
        Assert.Equal("Inactive", off!.Status);
        Assert.False(off.IsActive);

        Assert.True(await svc.ActivateAsync(id, TenantA, User));
        var on = await svc.GetByIdAsync(id, TenantA);
        Assert.Equal("Active", on!.Status);
        Assert.True(on.IsActive);
    }

    private sealed class FixedTenant(Guid tenantId) : ITenantProvider
    {
        public Guid? GetTenantId() => tenantId;
    }
}
