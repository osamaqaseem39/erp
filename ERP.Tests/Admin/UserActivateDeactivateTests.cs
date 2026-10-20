using ERP.Application.Admin;
using ERP.Application.Core.Auth;
using ERP.Application.Core.Settings;
using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Shared.Abstractions;
using ERP.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Admin;

public class UserActivateDeactivateTests
{
    private static readonly Guid Tenant = Guid.NewGuid();

    private sealed class FixedTenant : ITenantProvider
    {
        public Guid? GetTenantId() => Tenant;
    }

    private static ERPDbContext Db(string? name = null) => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(name ?? Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Deactivate_Ends_Sessions_Activate_Restores()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = Db(dbName);
        var (hash, salt) = PasswordHelper.HashPassword("Correct-horse-1");
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            Username = "a@x.test",
            Email = "a@x.test",
            PrimaryEmail = "a@x.test",
            FirstName = "A",
            LastName = "B",
            FullName = "A B",
            PasswordHash = hash,
            PasswordSalt = salt,
            IsActive = true
        };
        db.Users.Add(user);
        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            UserId = user.Id,
            SessionId = Guid.NewGuid(),
            TokenHash = "abc",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            SessionExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var tenant = new FixedTenant();
        var sessions = new SessionStore(db, new SecurityPolicyProvider(db));
        var svc = new UserService(
            new ERP.Infrastructure.Repositories.Core.UserRepository(db, tenant),
            new ERP.Infrastructure.Repositories.Core.UserRoleRepository(db),
            db,
            new SecurityPolicyProvider(db),
            sessions);

        Assert.True(await svc.DeactivateAsync(user.Id));
        Assert.False((await db.Users.SingleAsync()).IsActive);
        Assert.All(await db.RefreshTokens.ToListAsync(), t => Assert.NotNull(t.RevokedAt));

        Assert.True(await svc.ActivateAsync(user.Id));
        Assert.True((await db.Users.SingleAsync()).IsActive);
    }
}
