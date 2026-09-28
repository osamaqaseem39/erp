using ERP.Shared.DTOs;
using ERP.Shared.DTOs.CRM;
using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id);
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<PagedResult<Customer>> GetPagedAsync(CustomerPagedQuery query, CancellationToken cancellationToken = default);
    Task<IEnumerable<Customer>> GetByStatusAsync(string status);
    Task<IEnumerable<Customer>> GetByIndustryAsync(string industry);
    Task<Customer?> GetByCodeAsync(string customerCode);
    Task<Guid> CreateAsync(Customer customer);
    Task<bool> UpdateAsync(Customer customer);
    Task<bool> DeleteAsync(Guid id);
}
