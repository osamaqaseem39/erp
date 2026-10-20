using ERP.Shared.DTOs;
using ERP.Shared.DTOs.CRM;
using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Customer>> GetAllAsync(Guid tenantId);
    Task<PagedResult<Customer>> GetPagedAsync(CustomerPagedQuery query, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Customer>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<Customer>> GetByIndustryAsync(string industry, Guid tenantId);
    Task<Customer?> GetByCodeAsync(string customerCode, Guid tenantId);
    Task<Guid> CreateAsync(Customer customer);
    Task<bool> UpdateAsync(Customer customer);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    /// <summary>Records that reference the customer, by kind (only kinds with at least one record).</summary>
    Task<IReadOnlyDictionary<string, int>> GetDependentCountsAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
}
