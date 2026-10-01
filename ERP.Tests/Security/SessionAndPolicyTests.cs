using ERP.Application.Admin;
using ERP.Application.Core;
using ERP.Application.Core.Auth;
using ERP.Application.Core.Settings;
using ERP.Domain.Admin;
using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Core;
using ERP.Shared.Configuration;
using ERP.Shared.DTOs.Admin;
using ERP.Shared.DTOs.Core;
using ERP.Shared.Exceptions;
using ERP.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Security;

/// <summary>Checkpoint 2 WP4: per-session hashed refresh tokens and the tenant's Security settings.</summary>
public class SessionAndPolicyTests
{
    private const string Password = "Correct-horse-1";
    private static readonly Guid Tenant = Guid.NewGuid();
    private readonly string _dbName = Guid.NewGuid().ToString();
    private readonly ManualClock _clock = new(new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero));
    private readonly User _user;

    static SessionAndPolicyTests()
    {
        JwtHelper.Configure(new JwtSettings { SecretKey = new string('k', 48), ExpiryMinutes = 60, RefreshTokenExpiryDays = 7 });
    }

    public SessionAndPolicyTests()
    {
        using var db = Db();
        var (hash, salt) = PasswordHelper.HashPassword(Password);
        _user = new User
        {
            TenantId = Tenant, Username = "u@x.test", Email = "u@x.test", PrimaryEmail = "u@x.test", FirstName = "F", LastName = "L",
            FullName = "F L", PasswordHash = hash, PasswordSalt = salt, IsActive = true,
        };
        db.Users.Add(_user);
        db.SaveChanges();
    }

    private ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>().UseInMemoryDatabase(_dbName).Options);

    private SessionStore Sessions(ERPDbContext db) => new(db, new SecurityPolicyProvider(db), clock: _clock);

    private AuthService Auth(ERPDbContext db) => new(db, Sessions(db), new SecurityPolicyProvider(db));

    private void SetSetting(string key, string value)
    {
        using var db = Db();
        db.SystemSettings.Add(new SystemSetting { TenantId = Tenant, SettingKey = key, SettingValue = value, Category = "Security" });
        db.SaveChanges();
    }

    private Task<AuthResponseDto?> SignIn(ERPDbContext db, string password = Password) =>
        Auth(db).LoginAsync(new LoginDto { PrimaryEmail = "u@x.test", Password = password, TenantId = Tenant });

    [Fact]
    public async Task Refresh_tokens_are_stored_only_as_hashes()
    {
        await using var db = Db();
        var response = await SignIn(db);

        var row = await db.RefreshTokens.SingleAsync();
        Assert.NotEqual(response!.RefreshToken, row.TokenHash);
        Assert.Equal(SessionStore.Hash(response.RefreshToken), row.TokenHash);
        Assert.False(await db.RefreshTokens.AnyAsync(t => t.TokenHash == response.RefreshToken));
        Assert.Null((await db.Users.SingleAsync()).RefreshToken);
    }

    [Fact]
    public async Task Each_sign_in_is_its_own_session()
    {
        await using var db = Db();
        var laptop = await SignIn(db);
        var phone = await SignIn(db);

        // Signing in on the phone doesn't end the laptop's session.
        Assert.NotNull(await Auth(db).RefreshTokenAsync(new RefreshTokenDto { RefreshToken = laptop!.RefreshToken }));
        Assert.NotNull(await Auth(db).RefreshTokenAsync(new RefreshTokenDto { RefreshToken = phone!.RefreshToken }));
    }

    [Fact]
    public async Task Refresh_rotates_the_token()
    {
        await using var db = Db();
        var first = await SignIn(db);
        var second = await Auth(db).RefreshTokenAsync(new RefreshTokenDto { RefreshToken = first!.RefreshToken });

        Assert.NotEqual(first.RefreshToken, second!.RefreshToken);
        Assert.NotNull(await Auth(db).RefreshTokenAsync(new RefreshTokenDto { RefreshToken = second.RefreshToken }));
    }

    [Fact]
    public async Task Reusing_a_replaced_token_ends_the_session()
    {
        await using var db = Db();
        var first = await SignIn(db);
        var second = await Auth(db).RefreshTokenAsync(new RefreshTokenDto { RefreshToken = first!.RefreshToken });
        _clock.Advance(TimeSpan.FromMinutes(2));

        await Assert.ThrowsAsync<UnauthorizedException>(() => Auth(db).RefreshTokenAsync(new RefreshTokenDto { RefreshToken = first.RefreshToken }));
        // The legitimate holder of the newer token is signed out too: the session was compromised.
        await Assert.ThrowsAsync<UnauthorizedException>(() => Auth(db).RefreshTokenAsync(new RefreshTokenDto { RefreshToken = second!.RefreshToken }));
        Assert.All(await db.RefreshTokens.ToListAsync(), t => Assert.Equal(SessionStore.ReasonReuse, t.RevokedReason));
    }

    [Fact]
    public async Task Two_tabs_refreshing_at_once_are_not_treated_as_theft()
    {
        await using var db = Db();
        var first = await SignIn(db);
        await Auth(db).RefreshTokenAsync(new RefreshTokenDto { RefreshToken = first!.RefreshToken });
        _clock.Advance(TimeSpan.FromSeconds(5));

        Assert.NotNull(await Auth(db).RefreshTokenAsync(new RefreshTokenDto { RefreshToken = first.RefreshToken }));
    }

    [Fact]
    public async Task Idle_sessions_expire_after_the_tenants_session_timeout()
    {
        SetSetting("security.sessionTimeoutMinutes", "15");
        await using var db = Db();
        var response = await SignIn(db);

        Assert.InRange(response!.ExpiresAt - DateTime.UtcNow, TimeSpan.FromMinutes(14), TimeSpan.FromMinutes(15));
        _clock.Advance(TimeSpan.FromMinutes(16));
        await Assert.ThrowsAsync<UnauthorizedException>(() => Auth(db).RefreshTokenAsync(new RefreshTokenDto { RefreshToken = response.RefreshToken }));
    }

    [Fact]
    public async Task Active_sessions_continue_past_the_timeout()
    {
        SetSetting("security.sessionTimeoutMinutes", "15");
        await using var db = Db();
        var token = (await SignIn(db))!.RefreshToken;

        for (var i = 0; i < 8; i++) // two hours of activity, refreshing every 15 minutes
        {
            _clock.Advance(TimeSpan.FromMinutes(14));
            token = (await Auth(db).RefreshTokenAsync(new RefreshTokenDto { RefreshToken = token }))!.RefreshToken;
        }
    }

    [Fact]
    public async Task Sign_out_ends_only_that_session()
    {
        await using var db = Db();
        var laptop = await SignIn(db);
        var phone = await SignIn(db);

        await Auth(db).LogoutByRefreshTokenAsync(laptop!.RefreshToken);

        await Assert.ThrowsAsync<UnauthorizedException>(() => Auth(db).RefreshTokenAsync(new RefreshTokenDto { RefreshToken = laptop.RefreshToken }));
        Assert.NotNull(await Auth(db).RefreshTokenAsync(new RefreshTokenDto { RefreshToken = phone!.RefreshToken }));
    }

    [Fact]
    public async Task Changing_the_password_ends_every_session()
    {
        await using var db = Db();
        var session = await SignIn(db);

        await Auth(db).ChangePasswordAsync(_user.Id, Password, "Another-strong-2");

        await Assert.ThrowsAsync<UnauthorizedException>(() => Auth(db).RefreshTokenAsync(new RefreshTokenDto { RefreshToken = session!.RefreshToken }));
    }

    [Fact]
    public async Task Lockout_follows_the_tenants_threshold_and_duration()
    {
        SetSetting("security.lockoutThreshold", "3");
        SetSetting("security.lockoutMinutes", "10");
        await using var db = Db();

        for (var i = 0; i < 3; i++)
            await Assert.ThrowsAsync<UnauthorizedException>(() => SignIn(db, "wrong"));

        var user = await db.Users.SingleAsync();
        Assert.True(user.IsLocked);
        Assert.InRange(user.LockedUntil!.Value - DateTime.UtcNow, TimeSpan.FromMinutes(9), TimeSpan.FromMinutes(10));
        var locked = await Assert.ThrowsAsync<UnauthorizedException>(() => SignIn(db));
        Assert.Equal("Account is locked", locked.Message);
    }

    [Fact]
    public async Task An_expired_lock_starts_the_count_again()
    {
        SetSetting("security.lockoutThreshold", "3");
        await using var db = Db();
        var user = await db.Users.SingleAsync();
        user.IsLocked = true;
        user.FailedLoginAttempts = 3;
        user.LockedUntil = DateTime.UtcNow.AddMinutes(-1);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<UnauthorizedException>(() => SignIn(db, "wrong"));

        Assert.False((await db.Users.SingleAsync()).IsLocked);
    }

    [Fact]
    public async Task Password_minimum_follows_the_tenants_setting()
    {
        SetSetting("security.passwordMinLength", "12");
        await using var db = Db();

        var error = await Assert.ThrowsAsync<ValidationException>(() => Auth(db).ChangePasswordAsync(_user.Id, Password, "Short-pw-1"));
        Assert.Equal(["newPassword"], error.FieldErrors!.Keys);
        Assert.Contains("12", error.Message);
        Assert.True(await Auth(db).ChangePasswordAsync(_user.Id, Password, "Long-enough-pw-1"));
    }

    [Fact]
    public async Task Administrators_cannot_set_a_password_below_the_minimum()
    {
        await using var db = Db();
        var service = new UserService(new UserRepository(db, new FixedTenant(Tenant)), new UserRoleRepository(db), db);

        await Assert.ThrowsAsync<ValidationException>(() => service.ResetPasswordAsync(new ResetUserPasswordDto { UserId = _user.Id, NewPassword = "seven77" }));
        await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(new ERP.Shared.DTOs.Admin.CreateUserDto { FirstName = "N", LastName = "U", Email = "n@x.test", Password = "seven77" }));
    }

    [Fact]
    public async Task Settings_out_of_range_fall_back_within_limits()
    {
        SetSetting("security.passwordMinLength", "3");
        SetSetting("security.lockoutThreshold", "not a number");
        await using var db = Db();

        var policy = await new SecurityPolicyProvider(db).GetAsync(Tenant);

        Assert.Equal(8, policy.PasswordMinLength);
        Assert.Equal(5, policy.LockoutThreshold);
    }

    private sealed class FixedTenant(Guid id) : ERP.Shared.Abstractions.ITenantProvider
    {
        public Guid? GetTenantId() => id;
    }

    private sealed class ManualClock(DateTimeOffset start) : TimeProvider
    {
        private DateTimeOffset _now = start;
        public override DateTimeOffset GetUtcNow() => _now;
        public void Advance(TimeSpan by) => _now += by;
    }
}
