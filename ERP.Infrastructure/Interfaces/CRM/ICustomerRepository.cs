using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id);
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<Customer> AddAsync(Customer entity);
    Task UpdateAsync(Customer entity);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<Customer>> GetByCompanyIdAsync(Guid companyId);
    Task<Customer?> GetByCodeAsync(string code);
}