using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP.Infrastructure.Security;

/// <summary>One permission the application enforces, e.g. Resource "crm.leads", Action "convert".</summary>
public sealed record PermissionDefinition(string Resource, string Action, string Description)
{
    public string Code => $"{Resource}.{Action}";
    public string Module => Resource.Split('.')[0];
}

/// <summary>The permissions the running application enforces (generated from its endpoints by the API).</summary>
public interface IPermissionCatalog
{
    IReadOnlyList<PermissionDefinition> Permissions { get; }
}

public interface IRbacSeeder
{
    /// <summary>Brings a tenant's permission list and built-in roles up to date. Safe to run repeatedly.</summary>
    Task<RbacSeedResult> SeedTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task SeedAllTenantsAsync(CancellationToken cancellationToken = default);
}

public sealed record RbacSeedResult(int PermissionsAdded, int RolesCreated, int UsersGivenReadOnly);

/// <summary>
/// Keeps every tenant's Permissions table in step with the catalog and maintains the built-in roles:
/// - Administrator (system): every permission, updated as new endpoints appear;
/// - Read-only (system): every "view" permission, updated likewise;
/// - Sales, Finance, HR, Operations: editable templates created once and then left to the tenant.
/// When Read-only is first created for a tenant, users with no role at all are given it, so turning
/// on permission checks doesn't leave existing users with nothing (Checkpoint 2, decision D5).
/// </summary>
public sealed class RbacSeeder : IRbacSeeder
{
    public const string Administrator = "Administrator";
    public const string ReadOnly = "Read-only";

    private static readonly (string Name, string Description, string[] Modules)[] Templates =
    [
        ("Sales", "Customers, leads, pipeline, quotes, orders and point of sale.", ["crm", "sales"]),
        ("Finance", "Accounts, invoices, bills, payments and banking.", ["finance"]),
        ("HR", "Employees, leave, time, payroll and performance.", ["hrm"]),
        ("Operations", "Inventory, procurement, manufacturing and projects.", ["inventory", "procurement", "manufacturing", "projects"]),
    ];

    private readonly ERPDbContext _db;
    private readonly IPermissionCatalog _catalog;
    private readonly ILogger<RbacSeeder> _logger;

    public RbacSeeder(ERPDbContext db, IPermissionCatalog catalog, ILogger<RbacSeeder> logger)
    {
        _db = db;
        _catalog = catalog;
        _logger = logger;
    }

    public async Task SeedAllTenantsAsync(CancellationToken cancellationToken = default)
    {
        var tenantIds = await _db.Tenants.AsNoTracking().Select(t => t.Id).ToListAsync(cancellationToken);
        foreach (var tenantId in tenantIds)
        {
            var result = await SeedTenantAsync(tenantId, cancellationToken);
            if (result != new RbacSeedResult(0, 0, 0))
                _logger.LogInformation("RBAC seed for tenant {TenantId}: {Result}", tenantId, result);
        }
    }

    public async Task<RbacSeedResult> SeedTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        // 1. Permissions
        var existing = await _db.Permissions.Where(p => p.TenantId == tenantId).ToListAsync(cancellationToken);
        var byCode = existing.ToDictionary(p => $"{p.ModuleName}.{p.ActionName}", StringComparer.OrdinalIgnoreCase);
        var added = 0;
        foreach (var definition in _catalog.Permissions)
        {
            if (byCode.TryGetValue(definition.Code, out var permission))
            {
                permission.IsActive = true;
                permission.Description = definition.Description;
                continue;
            }
            permission = new Permission
            {
                TenantId = tenantId,
                ModuleName = definition.Resource,
                ActionName = definition.Action,
                Description = definition.Description,
                IsActive = true,
            };
            _db.Permissions.Add(permission);
            byCode[definition.Code] = permission;
            added++;
        }
        // Endpoints that no longer exist: keep the row (history, role links) but mark it inactive.
        var current = _catalog.Permissions.Select(p => p.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var (code, permission) in byCode)
            if (!current.Contains(code)) permission.IsActive = false;
        await _db.SaveChangesAsync(cancellationToken);

        // 2. Roles
        var roles = await _db.Roles.Where(r => r.TenantId == tenantId).ToListAsync(cancellationToken);
        var rolesCreated = 0;
        Role Ensure(string name, string description, bool system)
        {
            var role = roles.FirstOrDefault(r => r.RoleName == name);
            if (role != null) return role;
            role = new Role { TenantId = tenantId, RoleName = name, Description = description, IsSystemRole = system, IsActive = true };
            _db.Roles.Add(role);
            roles.Add(role);
            rolesCreated++;
            return role;
        }

        var readOnlyIsNew = roles.All(r => r.RoleName != ReadOnly);
        var administrator = Ensure(Administrator, "Full access to everything in this organization.", system: true);
        var readOnly = Ensure(ReadOnly, "Can view everything, change nothing.", system: true);
        var templates = Templates.Select(t => (Role: Ensure(t.Name, t.Description, system: false), t.Modules)).ToList();
        await _db.SaveChangesAsync(cancellationToken);

        var active = byCode.Values.Where(p => p.IsActive).ToList();
        await SyncAsync(administrator, active, cancellationToken);
        await SyncAsync(readOnly, active.Where(p => p.ActionName == "view").ToList(), cancellationToken);
        foreach (var (role, modules) in templates)
        {
            // Templates are filled once (when empty); after that the tenant owns them.
            if (await _db.RolePermissions.AnyAsync(rp => rp.RoleId == role.Id, cancellationToken)) continue;
            await SyncAsync(role, active.Where(p => modules.Contains(p.ModuleName.Split('.')[0])).ToList(), cancellationToken);
        }

        // 3. Users with no role at all get Read-only, once, when that role is introduced.
        var usersGiven = 0;
        if (readOnlyIsNew)
        {
            var roleless = await _db.Users
                .Where(u => u.TenantId == tenantId && !_db.UserRoles.Any(ur => ur.UserId == u.Id))
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);
            foreach (var userId in roleless)
                _db.UserRoles.Add(new UserRole { TenantId = tenantId, UserId = userId, RoleId = readOnly.Id, AssignedAt = DateTime.UtcNow });
            usersGiven = roleless.Count;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return new RbacSeedResult(added, rolesCreated, usersGiven);
    }

    /// <summary>Makes the role hold exactly the given permissions.</summary>
    private async Task SyncAsync(Role role, IReadOnlyCollection<Permission> permissions, CancellationToken cancellationToken)
    {
        var current = await _db.RolePermissions.Where(rp => rp.RoleId == role.Id).ToListAsync(cancellationToken);
        var wanted = permissions.Select(p => p.Id).ToHashSet();
        _db.RolePermissions.RemoveRange(current.Where(rp => !wanted.Contains(rp.PermissionId)));
        var have = current.Select(rp => rp.PermissionId).ToHashSet();
        foreach (var permission in permissions.Where(p => !have.Contains(p.Id)))
            _db.RolePermissions.Add(new RolePermission { TenantId = role.TenantId, RoleId = role.Id, PermissionId = permission.Id, IsAllowed = true });
        await _db.SaveChangesAsync(cancellationToken);
    }
}
