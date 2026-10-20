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

public class TwoFactorTotpTests
{
    private const string Password = "Correct-horse-1";
    private static readonly Guid Tenant = Guid.NewGuid();
    private readonly string _dbName = Guid.NewGuid().ToString();
    private readonly Guid _userId;

    static TwoFactorTotpTests()
    {
        JwtHelper.Configure(new JwtSettings
        {
            SecretKey = new string('k', 48),
            ExpiryMinutes = 60,
            RefreshTokenExpiryDays = 7
        });
    }

    public TwoFactorTotpTests()
    {
        using var db = Db();
        var (hash, salt) = PasswordHelper.HashPassword(Password);
        var user = new User
        {
            TenantId = Tenant,
            Username = "2fa@x.test",
            Email = "2fa@x.test",
            PrimaryEmail = "2fa@x.test",
            FirstName = "Two",
            LastName = "Fa",
            FullName = "Two Fa",
            PasswordHash = hash,
            PasswordSalt = salt,
            IsActive = true
        };
        db.Users.Add(user);
        db.SaveChanges();
        _userId = user.Id;
    }

    private ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(_dbName).Options);

    private AuthService Auth(ERPDbContext db) =>
        new(db, new SessionStore(db, new SecurityPolicyProvider(db)), new SecurityPolicyProvider(db));

    [Fact]
    public void TotpHelper_RoundTrips_KnownCode()
    {
        var secret = TotpHelper.GenerateSecret();
        var code = TotpHelper.ComputeCode(secret);
        Assert.True(TotpHelper.Verify(secret, code));
        Assert.False(TotpHelper.Verify(secret, "000000"));
    }

    [Fact]
    public async Task Setup_Enable_Login_Requires_Then_Verify()
    {
        await using var db = Db();
        var auth = Auth(db);

        var setup = await auth.BeginTwoFactorSetupAsync(_userId);
        Assert.False(string.IsNullOrWhiteSpace(setup.Secret));
        Assert.StartsWith("otpauth://totp/", setup.OtpAuthUri);

        var code = TotpHelper.ComputeCode(setup.Secret);
        Assert.True(await auth.EnableTwoFactorAsync(_userId, code));
        Assert.True((await auth.GetTwoFactorStatusAsync(_userId)).Enabled);

        var login = await auth.LoginAsync(new LoginDto
        {
            PrimaryEmail = "2fa@x.test",
            Password = Password,
            TenantId = Tenant
        });
        Assert.NotNull(login);
        Assert.True(login!.RequiresTwoFactor);
        Assert.False(string.IsNullOrWhiteSpace(login.TwoFactorToken));
        Assert.True(string.IsNullOrEmpty(login.Token));

        var totp = TotpHelper.ComputeCode(setup.Secret);
        var full = await auth.VerifyTwoFactorLoginAsync(login.TwoFactorToken!, totp);
        Assert.NotNull(full);
        Assert.False(full!.RequiresTwoFactor);
        Assert.False(string.IsNullOrWhiteSpace(full.Token));
        Assert.False(string.IsNullOrWhiteSpace(full.RefreshToken));
    }

    [Fact]
    public async Task Disable_Requires_Password_And_Code()
    {
        await using var db = Db();
        var auth = Auth(db);
        var setup = await auth.BeginTwoFactorSetupAsync(_userId);
        await auth.EnableTwoFactorAsync(_userId, TotpHelper.ComputeCode(setup.Secret));

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            auth.DisableTwoFactorAsync(_userId, "wrong-password", TotpHelper.ComputeCode(setup.Secret)));

        Assert.True(await auth.DisableTwoFactorAsync(
            _userId, Password, TotpHelper.ComputeCode(setup.Secret)));
        Assert.False((await auth.GetTwoFactorStatusAsync(_userId)).Enabled);

        var login = await auth.LoginAsync(new LoginDto
        {
            PrimaryEmail = "2fa@x.test",
            Password = Password,
            TenantId = Tenant
        });
        Assert.NotNull(login);
        Assert.False(login!.RequiresTwoFactor);
        Assert.False(string.IsNullOrWhiteSpace(login.Token));
    }
}
