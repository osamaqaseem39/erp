using ERP.Domain.HRM;
using ERP.Shared;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.HRM;

namespace ERP.Infrastructure.Repositories.HRM;

public interface IEmployeeRepository
{
    Task<Guid> AddEmployeeAsync(Employee employee);
    Task UpdateEmployeeAsync(Employee employee);
    Task DeleteEmployeeAsync(Guid employeeId);
    Task<Employee?> GetEmployeeByIdAsync(Guid employeeId);
    Task<PagedResult<Employee>> GetAllEmployeesAsync(EmployeeQuery filter);
    Task<Employee?> GetEmployeeByCodeAsync(string employeeCode);
    Task<bool> IsEmployeeCodeUniqueAsync(string employeeCode, Guid? excludeEmployeeId = null);
}