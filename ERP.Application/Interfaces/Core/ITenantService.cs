using ERP.Shared.DTOs.Core;

namespace ERP.Application.Interfaces.Core;

public interface ITenantService
{
    Task<TenantDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<TenantDto>> GetAllAsync();
    Task<IEnumerable<TenantDto>> GetActiveAsync();
    Task<TenantDto?> GetByDomainAsync(string domain);
    Task<Guid> CreateAsync(CreateTenantDto dto);
    Task<bool> UpdateAsync(UpdateTenantDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> SuspendAsync(Guid id);
    Task<bool> ActivateAsync(Guid id);
}