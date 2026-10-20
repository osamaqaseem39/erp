using ERP.Infrastructure.Database;
using ERP.Infrastructure.Logging;
using ERP.Shared.DTOs.Admin;
using ERP.Shared.DTOs.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP.Application.Admin;

public interface IAdminDashboardService
{
    Task<AdminDashboardDto> GetSummaryAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

/// <summary>Admin home widgets from real rows — users/roles/sign-ins/permission audits.</summary>
public class AdminDashboardService : IAdminDashboardService
{
    private static readonly string[] PermissionEntities = ["Role", "RolePermission", "UserRole", "Permission"];

    private readonly ERPDbContext _db;
    private readonly TimeProvider _clock;
    private readonly ILogger<AdminDashboardService> _logger;

    public AdminDashboardService(ERPDbContext db, TimeProvider clock, ILogger<AdminDashboardService> logger)
    {
        _db = db;
        _clock = clock;
        _logger = logger;
    }

    public async Task<AdminDashboardDto> GetSummaryAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var now = _clock.GetUtcNow().UtcDateTime;
        return new AdminDashboardDto
        {
            Users = await Widget("users", () => UsersAsync(tenantId, now, cancellationToken)),
            Roles = await Widget("roles", () => RolesAsync(tenantId, cancellationToken)),
            SignIns = await Widget("signIns", () => SignInsAsync(tenantId, now, cancellationToken)),
            RecentPermissionChanges = await Widget("permissionChanges", () => PermissionChangesAsync(tenantId, cancellationToken)),
        };
    }

    private async Task<WidgetResult<T>> Widget<T>(string name, Func<Task<T>> compute)
    {
        try
        {
            return new WidgetResult<T> { Data = await compute() };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Admin dashboard widget {Widget} failed", name);
            return new WidgetResult<T> { Error = "This figure couldn't be calculated right now." };
        }
    }

    private async Task<AdminUsersWidget> UsersAsync(Guid tenantId, DateTime now, CancellationToken ct)
    {
        var users = await _db.Users.AsNoTracking()
            .Where(u => u.TenantId == tenantId)
            .Select(u => new { u.Id, u.IsActive, u.IsLocked, u.LockedUntil })
            .ToListAsync(ct);

        var withRole = await _db.UserRoles.AsNoTracking()
            .Where(ur => ur.TenantId == tenantId)
            .Select(ur => ur.UserId)
            .Distinct()
            .ToListAsync(ct);
        var withRoleSet = withRole.ToHashSet();

        return new AdminUsersWidget
        {
            Total = users.Count,
            Active = users.Count(u => u.IsActive),
            Inactive = users.Count(u => !u.IsActive),
            Locked = users.Count(u => u.IsLocked && u.LockedUntil > now),
            WithoutRole = users.Count(u => u.IsActive && !withRoleSet.Contains(u.Id)),
        };
    }

    private async Task<AdminRolesWidget> RolesAsync(Guid tenantId, CancellationToken ct)
    {
        var roles = await _db.Roles.AsNoTracking()
            .Where(r => r.TenantId == tenantId && r.IsActive)
            .Select(r => r.IsSystemRole)
            .ToListAsync(ct);
        return new AdminRolesWidget
        {
            Total = roles.Count,
            SystemRoles = roles.Count(s => s),
        };
    }

    private async Task<AdminSignInsWidget> SignInsAsync(Guid tenantId, DateTime now, CancellationToken ct)
    {
        var security = _db.SystemLogs.AsNoTracking()
            .Where(l => l.TenantId == tenantId && l.Source == SystemLogSources.Security);
        return new AdminSignInsWidget
        {
            SignInsLast7Days = await security.CountAsync(
                l => l.Message == "Signed in" && l.Timestamp >= now.AddDays(-7), ct),
            SignInsPrevious7Days = await security.CountAsync(
                l => l.Message == "Signed in" && l.Timestamp >= now.AddDays(-14) && l.Timestamp < now.AddDays(-7), ct),
            FailedSignInsLast7Days = await security.CountAsync(
                l => l.Message.StartsWith("Failed sign-in") && l.Timestamp >= now.AddDays(-7), ct),
        };
    }

    private async Task<List<ConfigChangeSummary>> PermissionChangesAsync(Guid tenantId, CancellationToken ct)
    {
        var changes = await _db.AuditLogs.AsNoTracking()
            .Where(a => a.TenantId == tenantId && PermissionEntities.Contains(a.EntityName))
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .Select(a => new ConfigChangeSummary
            {
                Id = a.Id,
                Timestamp = a.Timestamp,
                EntityName = a.EntityName,
                Action = a.Action,
                UserId = a.UserId
            })
            .ToListAsync(ct);

        var userIds = changes.Where(c => c.UserId.HasValue).Select(c => c.UserId!.Value).Distinct().ToList();
        var names = await _db.Users.AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, Name = u.FullName ?? u.PrimaryEmail ?? u.Email })
            .ToDictionaryAsync(u => u.Id, u => u.Name, ct);
        foreach (var change in changes)
            change.UserName = change.UserId is Guid id ? names.GetValueOrDefault(id) : null;
        return changes;
    }
}
