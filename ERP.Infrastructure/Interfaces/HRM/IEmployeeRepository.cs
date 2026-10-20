using ERP.Domain.HRM;

namespace ERP.Infrastructure.Interfaces.HRM;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Employee>> GetAllAsync(Guid tenantId);
    Task<Employee> AddAsync(Employee entity);
    Task UpdateAsync(Employee entity);
    Task DeleteAsync(Guid id, Guid tenantId);
    Task<bool> ExistsAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Employee>> GetByCompanyIdAsync(Guid companyId, Guid tenantId);
    Task<Employee?> GetByCodeAsync(string code, Guid tenantId);
}
