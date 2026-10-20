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

public class ForgotPasswordAndSessionsTests
{
    private const string Password = "Correct-horse-1";
    private static readonly Guid Tenant = Guid.NewGuid();
    private readonly string _dbName = Guid.NewGuid().ToString();
    private readonly User _user;
    private readonly CapturingPasswordResetNotifier _notifier = new();

    static ForgotPasswordAndSessionsTests()
    {
        JwtHelper.Configure(new JwtSettings { SecretKey = new string('k', 48), ExpiryMinutes = 60, RefreshTokenExpiryDays = 7 });
    }

    public ForgotPasswordAndSessionsTests()
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

    private SessionStore Sessions(ERPDbContext db) => new(db, new SecurityPolicyProvider(db));

    private AuthService Auth(ERPDbContext db) =>
        new(db, Sessions(db), new SecurityPolicyProvider(db), _notifier);

    [Fact]
    public async Task ForgotPassword_HashesToken_And_NotifierReceivesRaw()
    {
        await using var db = Db();
        var auth = Auth(db);

        Assert.True(await auth.ForgotPasswordAsync("reset@x.test", Tenant));
        Assert.Equal("reset@x.test", _notifier.LastEmail);
        Assert.False(string.IsNullOrWhiteSpace(_notifier.LastRawToken));

        var stored = (await db.Users.SingleAsync()).PasswordResetToken;
        Assert.Equal(SessionStore.Hash(_notifier.LastRawToken!), stored);
        Assert.NotEqual(_notifier.LastRawToken, stored);
    }

    [Fact]
    public async Task ForgotPassword_UnknownEmail_StillSucceeds_WithoutNotify()
    {
        await using var db = Db();
        var auth = Auth(db);
        Assert.True(await auth.ForgotPasswordAsync("nobody@x.test", Tenant));
        Assert.Null(_notifier.LastRawToken);
    }

    [Fact]
    public async Task CompletePasswordReset_WithToken_ChangesPassword_AndEndsSessions()
    {
        await using var db = Db();
        var auth = Auth(db);

        await auth.LoginAsync(new LoginDto { PrimaryEmail = "reset@x.test", Password = Password, TenantId = Tenant });
        Assert.True(await auth.ForgotPasswordAsync("reset@x.test", Tenant));

        const string next = "New-correct-horse-2";
        Assert.True(await auth.CompletePasswordResetAsync("reset@x.test", _notifier.LastRawToken!, next));

        var user = await db.Users.SingleAsync();
        Assert.Null(user.PasswordResetToken);
        Assert.True(PasswordHelper.VerifyPassword(next, user.PasswordHash, user.PasswordSalt));
        Assert.All(await db.RefreshTokens.ToListAsync(), t => Assert.NotNull(t.RevokedAt));

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            auth.LoginAsync(new LoginDto { PrimaryEmail = "reset@x.test", Password = Password, TenantId = Tenant }));
        Assert.NotNull(await auth.LoginAsync(new LoginDto { PrimaryEmail = "reset@x.test", Password = next, TenantId = Tenant }));
    }

    [Fact]
    public async Task ListAndRevoke_Sessions_PerDevice()
    {
        await using var db = Db();
        var auth = Auth(db);

        var laptop = await auth.LoginAsync(new LoginDto { PrimaryEmail = "reset@x.test", Password = Password, TenantId = Tenant });
        var phone = await auth.LoginAsync(new LoginDto { PrimaryEmail = "reset@x.test", Password = Password, TenantId = Tenant });
        Assert.NotNull(laptop);
        Assert.NotNull(phone);

        var sessions = await auth.ListSessionsAsync(_user.Id);
        Assert.Equal(2, sessions.Count);

        var revokeId = sessions[0].SessionId;
        Assert.True(await auth.RevokeSessionAsync(_user.Id, revokeId));
        Assert.False(await auth.RevokeSessionAsync(_user.Id, revokeId));

        var remaining = await auth.ListSessionsAsync(_user.Id);
        Assert.Single(remaining);
        Assert.NotEqual(revokeId, remaining[0].SessionId);
    }

    private sealed class CapturingPasswordResetNotifier : IPasswordResetNotifier
    {
        public string? LastEmail { get; private set; }
        public string? LastRawToken { get; private set; }

        public Task NotifyAsync(string email, Guid tenantId, string rawToken, DateTime expiresAtUtc, CancellationToken ct = default)
        {
            LastEmail = email;
            LastRawToken = rawToken;
            return Task.CompletedTask;
        }
    }

    private sealed class ManualClock(DateTimeOffset start) : TimeProvider
    {
        private DateTimeOffset _now = start;
        public override DateTimeOffset GetUtcNow() => _now;
        public void Advance(TimeSpan by) => _now += by;
    }
}
