using ERP.Domain;

namespace ERP.Infrastructure.Extensions;

public static class TenantQueryExtensions
{
    public static IQueryable<T> ApplyTenantFilter<T>(this IQueryable<T> query, Guid? tenantId)
        where T : BaseEntity
    {
        if (tenantId.HasValue && tenantId.Value != Guid.Empty)
        {
            return query.Where(e => e.TenantId == tenantId.Value);
        }

        return query;
    }
}
