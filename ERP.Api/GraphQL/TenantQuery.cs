using HotChocolate;
using ERP.Application.Core;
using ERP.Shared.DTOs.Core;

namespace ERP.Api.GraphQL;

public class TenantQuery
{
    public async Task<TenantDto?> GetTenant(Guid id, [Service] ITenantService tenantService)
    {
        return await tenantService.GetByIdAsync(id);
    }

    public async Task<IEnumerable<TenantDto>> GetAllTenants([Service] ITenantService tenantService)
    {
        return await tenantService.GetAllAsync();
    }

    public async Task<TenantDto?> GetTenantBySubdomain(string subdomain, [Service] ITenantService tenantService)
    {
        return await tenantService.GetBySubdomainAsync(subdomain);
    }
}