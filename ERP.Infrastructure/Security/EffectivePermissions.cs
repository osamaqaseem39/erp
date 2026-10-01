using System.Collections.Concurrent;
using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace ERP.Infrastructure.Security;

public interface IEffectivePermissions
{
    /// <summary>Permission codes the user holds through their roles in the tenant (cached briefly).</summary>
    Task<IReadOnlySet<string>> GetAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Drops cached permissions for everyone in the tenant (after role or permission changes).</summary>
    void Invalidate(Guid tenantId);
}

/// <summary>
/// UserRoles → RolePermissions → Permissions, cached per user for up to a minute and dropped at once
/// when any role, role assignment or permission in the tenant changes (see PermissionCacheInvalidator).
/// A permission code is stored as ModuleName + "." + ActionName (e.g. "crm.customers" + "view").
/// </summary>
public sealed class EffectivePermissions : IEffectivePermissions
{
    public static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);

    private readonly IServiceScopeFactory _scopes;
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _tenantTokens = new();

    public EffectivePermissions(IServiceScopeFactory scopes, IMemoryCache cache)
    {
        _scopes = scopes;
        _cache = cache;
    }

    public async Task<IReadOnlySet<string>> GetAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        var key = $"perm:{tenantId}:{userId}";
        if (_cache.TryGetValue(key, out IReadOnlySet<string>? cached) && cached != null) return cached;

        // Singleton cache, scoped DbContext: query in a scope of our own.
        IReadOnlySet<string> codes;
        using (var scope = _scopes.CreateScope())
        {
            codes = await Load(scope.ServiceProvider.GetRequiredService<ERPDbContext>(), tenantId, userId, cancellationToken);
        }
        var token = _tenantTokens.GetOrAdd(tenantId, _ => new CancellationTokenSource());
        _cache.Set(key, codes, new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(CacheDuration)
            .AddExpirationToken(new CancellationChangeToken(token.Token)));
        return codes;
    }

    public static async Task<IReadOnlySet<string>> Load(ERPDbContext db, Guid tenantId, Guid userId, CancellationToken cancellationToken)
    {
        var rows = await (
            from ur in db.UserRoles.AsNoTracking()
            join r in db.Roles.AsNoTracking() on ur.RoleId equals r.Id
            join rp in db.RolePermissions.AsNoTracking() on r.Id equals rp.RoleId
            join p in db.Permissions.AsNoTracking() on rp.PermissionId equals p.Id
            where ur.UserId == userId && r.TenantId == tenantId && p.TenantId == tenantId && r.IsActive && p.IsActive && rp.IsAllowed
            select new { p.ModuleName, p.ActionName }).Distinct().ToListAsync(cancellationToken);
        return rows.Select(x => $"{x.ModuleName}.{x.ActionName}").ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public void Invalidate(Guid tenantId)
    {
        if (_tenantTokens.TryRemove(tenantId, out var token))
        {
            token.Cancel();
            token.Dispose();
        }
    }
}

/// <summary>Clears cached permissions for a tenant whenever its roles, role assignments or permissions are saved.</summary>
public sealed class PermissionCacheInvalidator : SaveChangesInterceptor
{
    private static readonly HashSet<Type> RbacTypes = [typeof(Role), typeof(UserRole), typeof(RolePermission), typeof(Permission)];
    private readonly IEffectivePermissions _permissions;

    public PermissionCacheInvalidator(IEffectivePermissions permissions)
    {
        _permissions = permissions;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Invalidate(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Invalidate(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void Invalidate(DbContext? context)
    {
        if (context is null) return;
        var tenants = context.ChangeTracker.Entries()
            .Where(e => (e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted) && RbacTypes.Contains(e.Entity.GetType()))
            .Select(e => ((ERP.Domain.BaseEntity)e.Entity).TenantId)
            .Distinct()
            .ToList();
        foreach (var tenant in tenants) _permissions.Invalidate(tenant);
    }
}
