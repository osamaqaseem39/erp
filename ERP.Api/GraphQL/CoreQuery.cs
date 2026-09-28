using HotChocolate;
using ERP.Application.Core;
using ERP.Application.Interfaces.Core;
using ERP.Infrastructure.Services;
using ERP.Shared.DTOs.Core;

namespace ERP.Api.GraphQL;

public class CoreQuery
{
    // Tenant Queries
    public async Task<TenantDto?> GetTenant(Guid id, [Service] ITenantService tenantService)
    {
        return await tenantService.GetByIdAsync(id);
    }

    public async Task<IEnumerable<TenantDto>> GetAllTenants([Service] ITenantService tenantService)
    {
        return await tenantService.GetAllAsync();
    }

    public async Task<TenantDto?> GetTenantByDomain(string domain, [Service] ITenantService tenantService)
    {
        return await tenantService.GetByDomainAsync(domain);
    }

    // Audit Log Queries
    public async Task<IEnumerable<AuditLogDto>> GetAuditLogsByTenant(Guid tenantId, [Service] IAuditService auditService, int pageNumber = 1, int pageSize = 50)
    {
        return await auditService.GetAuditLogsAsync(tenantId, pageNumber, pageSize);
    }
}