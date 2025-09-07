using ERP.Domain.HRM;

namespace ERP.Infrastructure.Interfaces.HRM;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id);
    Task<IEnumerable<Employee>> GetAllAsync();
    Task<Employee> AddAsync(Employee entity);
    Task UpdateAsync(Employee entity);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<Employee>> GetByCompanyIdAsync(Guid companyId);
    Task<Employee?> GetByCodeAsync(string code);
}