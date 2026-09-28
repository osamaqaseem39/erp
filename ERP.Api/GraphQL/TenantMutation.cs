using HotChocolate;
using ERP.Application.Core;
using ERP.Application.Interfaces.Core;
using ERP.Shared.DTOs.Core;

namespace ERP.Api.GraphQL;

public class TenantMutation
{
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