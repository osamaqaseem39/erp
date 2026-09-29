using ERP.Domain;
using ERP.Infrastructure.Database;
using ERP.Shared.Abstractions;
using ERP.Shared.Exceptions;

namespace ERP.Infrastructure.Repositories;

/// <summary>
/// Base for repositories of tenant-owned data. Unlike the older CRM base, it fails closed:
/// with no tenant in the request there is no query at all (401), never an unfiltered one.
/// </summary>
public abstract class TenantScopedRepositoryBase
{
    protected readonly ERPDbContext Context;
    private readonly ITenantProvider _tenantProvider;

    protected TenantScopedRepositoryBase(ERPDbContext context, ITenantProvider tenantProvider)
    {
        Context = context;
        _tenantProvider = tenantProvider;
    }

    protected Guid CurrentTenantId =>
        _tenantProvider.GetTenantId() is Guid id && id != Guid.Empty
            ? id
            : throw new UnauthorizedException("Your session has no tenant. Please sign in again.");

    /// <summary>Rows of the current tenant only.</summary>
    protected IQueryable<T> Scoped<T>(IQueryable<T> query) where T : BaseEntity
    {
        var tenantId = CurrentTenantId;
        return query.Where(e => e.TenantId == tenantId);
    }

    /// <summary>Stamps the current tenant on a new row, ignoring whatever the caller set.</summary>
    protected T Owned<T>(T entity) where T : BaseEntity
    {
        entity.TenantId = CurrentTenantId;
        return entity;
    }
}
