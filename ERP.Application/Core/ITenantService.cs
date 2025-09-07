using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Core;

namespace ERP.Application.Core;

public interface ITenantService
{
    Task<TenantDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<TenantDto>> GetAllAsync();
    Task<PagedResult<TenantDto>> GetPagedAsync(int pageNumber, int pageSize);
    Task<TenantDto?> GetBySubdomainAsync(string subdomain);
    Task<Guid> CreateAsync(CreateTenantDto dto);
    Task<bool> UpdateAsync(UpdateTenantDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}