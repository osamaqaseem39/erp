using ERP.Domain.HRM;

namespace ERP.Infrastructure.Repositories.HRM;

public interface IEmploymentContractRepository
{
    Task<EmploymentContract?> GetByIdAsync(Guid id);
    Task<IEnumerable<EmploymentContract>> GetAllAsync();
    Task<IEnumerable<EmploymentContract>> GetByEmployeeAsync(Guid employeeId);
    Task<IEnumerable<EmploymentContract>> GetByStatusAsync(string status);
    Task<Guid> CreateAsync(EmploymentContract employmentContract);
    Task<bool> UpdateAsync(EmploymentContract employmentContract);
    Task<bool> DeleteAsync(Guid id);
}