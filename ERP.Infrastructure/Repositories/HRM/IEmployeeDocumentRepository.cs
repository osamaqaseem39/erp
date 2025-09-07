using ERP.Domain.HRM;

namespace ERP.Infrastructure.Repositories.HRM;

public interface IEmployeeDocumentRepository
{
    Task<EmployeeDocument?> GetByIdAsync(Guid id);
    Task<IEnumerable<EmployeeDocument>> GetAllAsync();
    Task<IEnumerable<EmployeeDocument>> GetByEmployeeAsync(Guid employeeId);
    Task<IEnumerable<EmployeeDocument>> GetByTypeAsync(string documentType);
    Task<Guid> CreateAsync(EmployeeDocument employeeDocument);
    Task<bool> UpdateAsync(EmployeeDocument employeeDocument);
    Task<bool> DeleteAsync(Guid id);
}