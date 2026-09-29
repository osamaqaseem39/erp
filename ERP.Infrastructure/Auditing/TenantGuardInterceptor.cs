using ERP.Domain;
using ERP.Domain.Core;
using ERP.Shared.Abstractions;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ERP.Infrastructure.Auditing;

/// <summary>
/// Last line of defence for tenant isolation on writes, independent of any service or repository:
/// - new rows without a tenant get the caller's tenant;
/// - a non-SuperAdmin caller can never insert, update or delete another tenant's rows.
/// Many services still copy TenantId from request bodies; this makes that harmless.
/// Requests without a tenant (sign-in, first-run setup) are not affected.
/// </summary>
public sealed class TenantGuardInterceptor : SaveChangesInterceptor
{
    /// <summary>Platform-level records that are not owned by a tenant.</summary>
    private static readonly HashSet<Type> PlatformTypes = [typeof(Tenant), typeof(Plan)];

    private readonly IAuditContext _request;

    public TenantGuardInterceptor(IAuditContext request)
    {
        _request = request;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Guard(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Guard(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Guard(DbContext? context)
    {
        if (context is null || _request.TenantId is not Guid tenantId || tenantId == Guid.Empty) return;

        context.ChangeTracker.DetectChanges();
        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted)) continue;
            if (PlatformTypes.Contains(entry.Entity.GetType())) continue;

            if (entry.State == EntityState.Added && entry.Entity.TenantId == Guid.Empty)
            {
                entry.Entity.TenantId = tenantId;
                continue;
            }

            // For updates, check the tenant the row had when it was loaded, not a value the caller may have changed.
            var owner = entry.State == EntityState.Added ? entry.Entity.TenantId : (Guid)entry.Property(nameof(BaseEntity.TenantId)).OriginalValue!;
            var changedOwner = entry.State == EntityState.Modified && entry.Entity.TenantId != owner;
            if ((owner != tenantId || changedOwner) && !_request.IsSuperAdmin)
            {
                throw new ForbiddenException("You can only change records that belong to your own organization.");
            }
        }
    }
}
