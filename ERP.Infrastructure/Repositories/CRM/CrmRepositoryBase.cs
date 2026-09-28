using ERP.Domain;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Extensions;
using ERP.Shared.Abstractions;

namespace ERP.Infrastructure.Repositories.CRM;

public abstract class CrmRepositoryBase
{
    protected readonly ERPDbContext Context;
    private readonly ITenantProvider _tenantProvider;

    protected CrmRepositoryBase(ERPDbContext context, ITenantProvider tenantProvider)
    {
        Context = context;
        _tenantProvider = tenantProvider;
    }

    protected Guid? TenantId => _tenantProvider.GetTenantId();

    protected IQueryable<T> ApplyTenant<T>(IQueryable<T> query) where T : BaseEntity
        => query.ApplyTenantFilter(TenantId);

    protected bool MatchesTenant(BaseEntity entity)
        => !TenantId.HasValue || TenantId == Guid.Empty || entity.TenantId == TenantId;
}
