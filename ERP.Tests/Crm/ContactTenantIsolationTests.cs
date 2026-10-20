using ERP.Application.CRM;
using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.CRM;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.CRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Crm;

public class ContactTenantIsolationTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static ContactService Svc(ERPDbContext db) =>
        new(new ContactRepository(db, new FixedTenant(TenantA)));

    [Fact]
    public async Task Create_Stamps_Tenant_And_Activate_Deactivate()
    {
        await using var db = Db();
        var svc = Svc(db);
        var id = await svc.CreateAsync(new CreateContactDto
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com"
        }, TenantA, User);

        Assert.NotEqual(Guid.Empty, id);
        var row = await db.Contacts.SingleAsync(c => c.Id == id);
        Assert.Equal(TenantA, row.TenantId);
        Assert.True(row.IsActive);

        Assert.True(await svc.DeactivateAsync(id, TenantA, User));
        Assert.False((await svc.GetByIdAsync(id, TenantA))!.IsActive);
        Assert.True(await svc.ActivateAsync(id, TenantA, User));
        Assert.True((await svc.GetByIdAsync(id, TenantA))!.IsActive);
    }

    [Fact]
    public async Task Cross_Tenant_And_Empty_Fail_Closed()
    {
        await using var db = Db();
        var id = Guid.NewGuid();
        db.Contacts.Add(new Contact
        {
            Id = id,
            TenantId = TenantB,
            FirstName = "Secret",
            LastName = "Contact",
            Email = "x@y.z",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var svc = Svc(db);
        Assert.Null(await svc.GetByIdAsync(id, TenantA));
        Assert.Empty(await svc.GetAllAsync(TenantA));
        Assert.Empty(await svc.GetAllAsync(Guid.Empty));
        Assert.False(await svc.UpdateAsync(new UpdateContactDto { Id = id, FirstName = "Hijack" }, TenantA, User));
        Assert.False(await svc.DeleteAsync(id, TenantA));
        Assert.False(await svc.ActivateAsync(id, Guid.Empty, User));
    }

    private sealed class FixedTenant(Guid tenantId) : ITenantProvider
    {
        public Guid? GetTenantId() => tenantId;
    }
}
