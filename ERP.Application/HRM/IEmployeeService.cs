using ERP.Shared.DTOs.HRM;
using ERP.Shared.DTOs;

namespace ERP.Application.HRM;

public interface IEmployeeService
{
    Task<EmployeeDto> AddEmployeeAsync(CreateEmployeeDto dto, Guid tenantId, Guid userId);
    Task<EmployeeDto> UpdateEmployeeAsync(UpdateEmployeeDto dto, Guid tenantId, Guid userId);
    Task DeleteEmployeeAsync(Guid employeeId, Guid tenantId);
    Task<EmployeeDto> GetEmployeeByIdAsync(Guid employeeId, Guid tenantId);
    Task<PagedResult<EmployeeListDto>> GetEmployeesAsync(EmployeeQuery filter, Guid tenantId);
    Task<EmployeeDto> GetEmployeeByCodeAsync(string employeeCode, Guid tenantId);
    Task<bool> IsEmployeeCodeUniqueAsync(string employeeCode, Guid tenantId, Guid? excludeEmployeeId = null);
}
