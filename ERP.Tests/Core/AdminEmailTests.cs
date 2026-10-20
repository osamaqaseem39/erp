using ERP.Application.Core;
using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Core;
using ERP.Shared.DTOs.Core;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Core;

public class AdminEmailTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    private static ERPDbContext Db() =>
        new(new DbContextOptionsBuilder<ERPDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static AdminEmailService Service(ERPDbContext db) => new(new AdminEmailRepository(db));

    [Fact]
    public async Task CreateListAndSoftDelete()
    {
        await using var db = Db();
        var svc = Service(db);

        var created = await svc.CreateAsync(new CreateAdminEmailDto { Email = "ops@acme.test", Type = "Support" }, Tenant, UserId);
        Assert.Equal("ops@acme.test", created.Email);
        Assert.Equal("Support", created.Type);

        var all = await svc.GetAllAsync(Tenant, isActive: true);
        Assert.Single(all);

        await svc.DeleteAsync(created.Id, Tenant, UserId);
        Assert.Empty(await svc.GetAllAsync(Tenant, isActive: true));
        Assert.Single(await svc.GetAllAsync(Tenant, isActive: false));
    }

    [Fact]
    public async Task RejectsDuplicateActiveEmail()
    {
        await using var db = Db();
        var svc = Service(db);
        await svc.CreateAsync(new CreateAdminEmailDto { Email = "a@acme.test", Type = "Finance" }, Tenant, UserId);

        await Assert.ThrowsAsync<ValidationException>(() =>
            svc.CreateAsync(new CreateAdminEmailDto { Email = "A@acme.test", Type = "Notification" }, Tenant, UserId));
    }

    [Fact]
    public async Task TenantIsolation()
    {
        await using var db = Db();
        var svc = Service(db);
        var other = Guid.NewGuid();
        await svc.CreateAsync(new CreateAdminEmailDto { Email = "a@acme.test", Type = "Other" }, Tenant, UserId);

        Assert.Empty(await svc.GetAllAsync(other));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            svc.UpdateAsync(db.AdminEmails.Single().Id, new UpdateAdminEmailDto { Type = "Finance" }, other, UserId));
    }
}
