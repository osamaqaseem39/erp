using ERP.Shared.DTOs;
using ERP.Shared.DTOs.CRM;

namespace ERP.Application.Interfaces.CRM;

public interface ICustomerService
{
    Task<CustomerDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<CustomerDto>> GetAllAsync();
    Task<PagedResult<CustomerDto>> GetPagedAsync(CustomerPagedQuery query, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(CreateCustomerDto dto);
    Task<bool> UpdateAsync(UpdateCustomerDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<CustomerDto>> SearchAsync(string searchTerm);
    Task<IEnumerable<CustomerDto>> GetByStatusAsync(string status);
    Task<IEnumerable<CustomerDto>> GetByIndustryAsync(string industry);
}