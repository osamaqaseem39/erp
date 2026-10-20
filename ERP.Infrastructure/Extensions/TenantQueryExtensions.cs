using ERP.Domain;

namespace ERP.Infrastructure.Extensions;

public static class TenantQueryExtensions
{
    /// <summary>
    /// Tenant fail-closed: missing or empty tenant yields no rows (never unfiltered).
    /// </summary>
    public static IQueryable<T> ApplyTenantFilter<T>(this IQueryable<T> query, Guid? tenantId)
        where T : BaseEntity
    {
        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return query.Where(_ => false);

        return query.Where(e => e.TenantId == tenantId.Value);
    }
}
