using ERP.Domain.HRM;

namespace ERP.Infrastructure.Repositories.HRM;

public interface IEmployeeAddressRepository
{
    Task<EmployeeAddress?> GetByIdAsync(Guid id);
    Task<IEnumerable<EmployeeAddress>> GetAllAsync();
    Task<IEnumerable<EmployeeAddress>> GetByEmployeeAsync(Guid employeeId);
    Task<IEnumerable<EmployeeAddress>> GetByTypeAsync(string addressType);
    Task<Guid> CreateAsync(EmployeeAddress employeeAddress);
    Task<bool> UpdateAsync(EmployeeAddress employeeAddress);
    Task<bool> DeleteAsync(Guid id);
}