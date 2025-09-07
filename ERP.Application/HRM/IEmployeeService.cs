using ERP.Shared.DTOs.HRM;
using ERP.Shared.DTOs;

namespace ERP.Application.HRM;

public interface IEmployeeService
{
    Task<EmployeeDto> AddEmployeeAsync(CreateEmployeeDto dto);
    Task<EmployeeDto> UpdateEmployeeAsync(UpdateEmployeeDto dto);
    Task DeleteEmployeeAsync(Guid employeeId);
    Task<EmployeeDto> GetEmployeeByIdAsync(Guid employeeId);
    Task<PagedResult<EmployeeListDto>> GetEmployeesAsync(EmployeeQuery filter);
    Task<EmployeeDto> GetEmployeeByCodeAsync(string employeeCode);
    Task<bool> IsEmployeeCodeUniqueAsync(string employeeCode, Guid? excludeEmployeeId = null);
}