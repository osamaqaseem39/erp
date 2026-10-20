using ERP.Application.Core;
using ERP.Application.Core.Auth;
using ERP.Application.Core.Settings;
using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Shared.Configuration;
using ERP.Shared.DTOs.Core;
using ERP.Shared.Exceptions;
using ERP.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Security;

public class ForgotPasswordAndSessionTests
{
    private const string Password = "Correct-horse-1";
    private static readonly Guid Tenant = Guid.NewGuid();
    private readonly string _dbName = Guid.NewGuid().ToString();
    private readonly User _user;
    private readonly CapturingPasswordResetNotifier _notifier = new();

    static ForgotPasswordAndSessionTests()
    {
        JwtHelper.Configure(new JwtSettings { SecretKey = new string('k', 48), ExpiryMinutes = 60, RefreshTokenExpiryDays = 7 });
    }

    public ForgotPasswordAndSessionTests()
    {
        using var db = Db();
        var (hash, salt) = PasswordHelper.HashPassword(Password);
        _user = new User
        {
            TenantId = Tenant,
            Username = "reset@x.test",
            Email = "reset@x.test",
            PrimaryEmail = "reset@x.test",
            FirstName = "R",
            LastName = "U",
            FullName = "R U",
            PasswordHash = hash,
            PasswordSalt = salt,
            IsActive = true,
        };
        db.Users.Add(_user);
        db.SaveChanges();
    }

    private ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>().UseInMemoryDatabase(_dbName).Options);

    private AuthService Auth(ERPDbContext db) =>
        new(db, new SessionStore(db, new SecurityPolicyProvider(db)), new SecurityPolicyProvider(db), _notifier);

    [Fact]
    public async Task Forgot_stores_hashed_token_and_notifier_gets_raw()
    {
        await using var db = Db();
        Assert.True(await Auth(db).ForgotPasswordAsync("reset@x.test", Tenant));

        var user = await db.Users.SingleAsync();
        Assert.NotNull(_notifier.LastToken);
        Assert.NotEqual(_notifier.LastToken, user.PasswordResetToken);
        Assert.Equal(SessionStore.Hash(_notifier.LastToken!), user.PasswordResetToken);
    }

    [Fact]
    public async Task Complete_reset_with_raw_token_updates_password_and_ends_sessions()
    {
        await using var db = Db();
        var auth = Auth(db);
        await auth.LoginAsync(new LoginDto { PrimaryEmail = "reset@x.test", Password = Password, TenantId = Tenant });
        Assert.True(await db.RefreshTokens.AnyAsync(t => t.RevokedAt == null));

        await auth.ForgotPasswordAsync("reset@x.test", Tenant);
        Assert.True(await auth.CompletePasswordResetAsync("reset@x.test", _notifier.LastToken!, "Brand-new-pass-9"));

        Assert.Null((await db.Users.SingleAsync()).PasswordResetToken);
        Assert.False(await db.RefreshTokens.AnyAsync(t => t.RevokedAt == null));

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            auth.LoginAsync(new LoginDto { PrimaryEmail = "reset@x.test", Password = Password, TenantId = Tenant }));

        Assert.NotNull(await auth.LoginAsync(new LoginDto
        {
            PrimaryEmail = "reset@x.test",
            Password = "Brand-new-pass-9",
            TenantId = Tenant
        }));
    }

    [Fact]
    public async Task Forgot_unknown_email_still_succeeds_without_token()
    {
        await using var db = Db();
        _notifier.Clear();
        Assert.True(await Auth(db).ForgotPasswordAsync("nobody@x.test", Tenant));
        Assert.Null(_notifier.LastToken);
    }

    [Fact]
    public async Task List_and_revoke_sessions()
    {
        await using var db = Db();
        var auth = Auth(db);
        var a = await auth.LoginAsync(new LoginDto { PrimaryEmail = "reset@x.test", Password = Password, TenantId = Tenant });
        var b = await auth.LoginAsync(new LoginDto { PrimaryEmail = "reset@x.test", Password = Password, TenantId = Tenant });
        Assert.NotNull(a);
        Assert.NotNull(b);

        var sessions = await auth.ListSessionsAsync(_user.Id);
        Assert.Equal(2, sessions.Count);

        var victim = sessions[0].SessionId;
        Assert.True(await auth.RevokeSessionAsync(_user.Id, victim));

        sessions = await auth.ListSessionsAsync(_user.Id);
        Assert.Single(sessions);
        Assert.DoesNotContain(sessions, s => s.SessionId == victim);
    }
}
