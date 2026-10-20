using ERP.Shared.DTOs;
using ERP.Shared.DTOs.CRM;

namespace ERP.Application.Interfaces.CRM;

public interface ICustomerService
{
    Task<CustomerDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<CustomerDto>> GetAllAsync(Guid tenantId);
    Task<PagedResult<CustomerDto>> GetPagedAsync(CustomerPagedQuery query, Guid tenantId, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(CreateCustomerDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateCustomerDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> ActivateAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> DeactivateAsync(Guid id, Guid tenantId, Guid userId);
    Task<IEnumerable<CustomerDto>> SearchAsync(string searchTerm, Guid tenantId);
    Task<IEnumerable<CustomerDto>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<CustomerDto>> GetByIndustryAsync(string industry, Guid tenantId);
}
