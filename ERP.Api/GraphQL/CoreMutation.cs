using HotChocolate;
using ERP.Application.Core;
using ERP.Application.Interfaces.Core;
using ERP.Shared.DTOs.Core;

namespace ERP.Api.GraphQL;

// Tenant administration and cross-tenant audit reads: SuperAdmin only.
[HotChocolate.Authorization.Authorize(Roles = new[] { "SuperAdmin", "SUPER_ADMIN" })]
public class CoreMutation
{
    // Tenant Mutations
    public async Task<Guid> AddTenant(CreateTenantDto input, [Service] ITenantService tenantService)
    {
        return await tenantService.CreateAsync(input);
    }

    public async Task<bool> UpdateTenant(UpdateTenantDto input, [Service] ITenantService tenantService)
    {
        return await tenantService.UpdateAsync(input);
    }

    public async Task<bool> DeleteTenant(Guid id, [Service] ITenantService tenantService)
    {
        return await tenantService.DeleteAsync(id);
    }
}