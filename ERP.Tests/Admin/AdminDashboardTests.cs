using ERP.Application.Admin;
using ERP.Domain.Admin;
using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ERP.Tests.Admin;

public class AdminDashboardTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid Other = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 20, 12, 0, 0, DateTimeKind.Utc);
    private readonly ERPDbContext _db = new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private AdminDashboardService Service() =>
        new(_db, new FixedClock(new DateTimeOffset(Now)), NullLogger<AdminDashboardService>.Instance);

    [Fact]
    public async Task Users_Counts_Active_Inactive_Locked_And_WithoutRole()
    {
        var withRole = NewUser(Tenant, active: true);
        var noRole = NewUser(Tenant, active: true);
        var inactive = NewUser(Tenant, active: false);
        var locked = NewUser(Tenant, active: true, locked: true);
        var otherTenant = NewUser(Other, active: true);
        _db.Users.AddRange(withRole, noRole, inactive, locked, otherTenant);

        var role = new Role
        {
            TenantId = Tenant,
            Name = "Clerk",
            RoleName = "Clerk",
            IsActive = true,
            IsSystemRole = false
        };
        _db.Roles.Add(role);
        await _db.SaveChangesAsync();

        _db.UserRoles.Add(new UserRole
        {
            TenantId = Tenant,
            UserId = withRole.Id,
            RoleId = role.Id,
            IsActive = true
        });
        await _db.SaveChangesAsync();

        var summary = await Service().GetSummaryAsync(Tenant);
        Assert.Null(summary.Users.Error);
        var users = summary.Users.Data!;
        Assert.Equal(4, users.Total);
        Assert.Equal(3, users.Active);
        Assert.Equal(1, users.Inactive);
        Assert.Equal(1, users.Locked);
        Assert.Equal(2, users.WithoutRole); // noRole + locked (active, no role)
    }

    [Fact]
    public async Task Roles_And_SignIns_Are_TenantScoped()
    {
        _db.Roles.AddRange(
            new Role { TenantId = Tenant, Name = "Admin", RoleName = "Admin", IsActive = true, IsSystemRole = true },
            new Role { TenantId = Tenant, Name = "Ops", RoleName = "Ops", IsActive = true, IsSystemRole = false },
            new Role { TenantId = Other, Name = "X", RoleName = "X", IsActive = true, IsSystemRole = true });
        _db.SystemLogs.AddRange(
            Log("Signed in", Now.AddDays(-1)),
            Log("Signed in", Now.AddDays(-10)),
            Log("Failed sign-in: wrong password", Now.AddDays(-2)),
            new SystemLog
            {
                TenantId = Other,
                LogLevel = "Information",
                Source = SystemLogSources.Security,
                Message = "Signed in",
                Timestamp = Now.AddDays(-1)
            });
        await _db.SaveChangesAsync();

        var summary = await Service().GetSummaryAsync(Tenant);
        Assert.Equal((2, 1), (summary.Roles.Data!.Total, summary.Roles.Data.SystemRoles));
        Assert.Equal((1, 1, 1), (
            summary.SignIns.Data!.SignInsLast7Days,
            summary.SignIns.Data.SignInsPrevious7Days,
            summary.SignIns.Data.FailedSignInsLast7Days));
    }

    [Fact]
    public async Task RecentPermissionChanges_Lists_RoleAudits()
    {
        var actor = NewUser(Tenant, active: true);
        _db.Users.Add(actor);
        _db.AuditLogs.Add(new AuditLog
        {
            TenantId = Tenant,
            EntityName = "RolePermission",
            Action = "Update",
            Timestamp = Now.AddMinutes(-5),
            UserId = actor.Id
        });
        await _db.SaveChangesAsync();

        var changes = (await Service().GetSummaryAsync(Tenant)).RecentPermissionChanges.Data!;
        Assert.Single(changes);
        Assert.Equal("RolePermission", changes[0].EntityName);
        Assert.Equal(actor.Id, changes[0].UserId);
    }

    private static User NewUser(Guid tenant, bool active, bool locked = false) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenant,
        Username = Guid.NewGuid().ToString("N")[..8],
        Email = "u@x.test",
        PrimaryEmail = "u@x.test",
        FirstName = "U",
        LastName = "X",
        FullName = "U X",
        PasswordHash = "h",
        PasswordSalt = "s",
        IsActive = active,
        IsLocked = locked,
        LockedUntil = locked ? Now.AddMinutes(30) : null,
    };

    private static SystemLog Log(string message, DateTime at) => new()
    {
        TenantId = Tenant,
        LogLevel = "Information",
        Source = SystemLogSources.Security,
        Message = message,
        Timestamp = at
    };

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
