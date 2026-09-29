using ERP.Application.Core;
using ERP.Domain.Admin;
using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ERP.Tests.Core;

public class CoreDashboardTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid Other = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 9, 29, 12, 0, 0, DateTimeKind.Utc);
    private readonly ERPDbContext _db = new(new DbContextOptionsBuilder<ERPDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private CoreDashboardService Service() =>
        new(_db, new FixedClock(new DateTimeOffset(Now)), NullLogger<CoreDashboardService>.Instance);

    private static User UserSignedIn(Guid tenant, DateTime? lastSignIn, bool locked = false) => new()
    {
        TenantId = tenant, Username = Guid.NewGuid().ToString(), Email = "u@x.test", PrimaryEmail = "u@x.test",
        FirstName = "U", LastName = "X", FullName = "U X", PasswordHash = "h", PasswordSalt = "s", LastLoginDate = lastSignIn,
        IsLocked = locked, LockedUntil = locked ? Now.AddMinutes(10) : null,
    };

    [Fact]
    public async Task CountsRealUsersActivityForTheTenantOnly()
    {
        _db.Users.AddRange(
            UserSignedIn(Tenant, Now.AddDays(-1)),
            UserSignedIn(Tenant, Now.AddDays(-10)),
            UserSignedIn(Tenant, null, locked: true),
            UserSignedIn(Other, Now.AddDays(-1)));
        await _db.SaveChangesAsync();

        var users = (await Service().GetSummaryAsync(Tenant, null, includePlatform: false)).Users.Data!;

        Assert.Equal((3, 1, 1, 1), (users.TotalUsers, users.ActiveLast7Days, users.ActivePrevious7Days, users.LockedAccounts));
    }

    [Fact]
    public async Task CountsErrorsAndFailedSignInsInTheirWindows()
    {
        _db.SystemLogs.AddRange(
            Log("Error", SystemLogSources.Api, "POST /x failed (500)", Now.AddHours(-1)),
            Log("Error", SystemLogSources.Api, "GET /y failed (500)", Now.AddHours(-30)),
            Log("Warning", SystemLogSources.Security, "Failed sign-in: wrong password", Now.AddHours(-2)),
            Log("Warning", SystemLogSources.Security, "Account locked after repeated failed sign-ins", Now.AddDays(-2)));
        await _db.SaveChangesAsync();

        var summary = await Service().GetSummaryAsync(Tenant, null, includePlatform: false);

        Assert.Equal((1, 1), (summary.Errors.Data!.ErrorsLast24h, summary.Errors.Data.ErrorsPrevious24h));
        Assert.Equal("POST /x failed (500)", summary.Errors.Data.Latest[0].Message);
        Assert.Equal((1, 1), (summary.Security.Data!.FailedSignInsLast24h, summary.Security.Data.Lockouts7Days));
    }

    [Fact]
    public async Task SetupChecklistReflectsWhatIsActuallyConfigured()
    {
        var company = new Company { TenantId = Tenant, CompanyName = "Acme", LegalName = "Acme Ltd", Country = "Pakistan", AddressLine1 = "1 Main", Currency = "PKR", TimeZone = "Asia/Karachi" };
        _db.Companies.Add(company);
        _db.SystemSettings.Add(new SystemSetting { TenantId = Tenant, SettingKey = "preferences.locale", SettingValue = "en-PK" });
        await _db.SaveChangesAsync();

        var setup = (await Service().GetSummaryAsync(Tenant, company.Id, includePlatform: false)).Setup.Data!;

        Assert.Equal(
            new[] { ("company", true), ("regional", true), ("numbering", false), ("email", false) },
            setup.Select(s => (s.Key, s.Done)));
    }

    [Fact]
    public async Task TenantOverviewIsOnlyIncludedForThePlatform()
    {
        Assert.Null((await Service().GetSummaryAsync(Tenant, null, includePlatform: false)).Tenants);
        Assert.NotNull((await Service().GetSummaryAsync(Tenant, null, includePlatform: true)).Tenants);
    }

    private static SystemLog Log(string level, string source, string message, DateTime at) =>
        new() { TenantId = Tenant, LogLevel = level, Source = source, Message = message, Timestamp = at };

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
