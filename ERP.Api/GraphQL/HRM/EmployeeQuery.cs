using ERP.Application.HRM;
using ERP.Shared.DTOs.HRM;
using ERP.Shared.DTOs;
using HotChocolate;

namespace ERP.Api.GraphQL.HRM;

[ExtendObjectType("Query")]
public class EmployeeQuery
{
    public async Task<EmployeeDto?> GetEmployeeById(
        [Service] IEmployeeService employeeService,
        Guid employeeId)
    {
        return await employeeService.GetEmployeeByIdAsync(employeeId);
    }

    public async Task<EmployeeDto?> GetEmployeeByCode(
        [Service] IEmployeeService employeeService,
        string employeeCode)
    {
        return await employeeService.GetEmployeeByCodeAsync(employeeCode);
    }

    public async Task<PagedResult<EmployeeListDto>> GetAllEmployees(
        [Service] IEmployeeService employeeService,
        ERP.Shared.DTOs.HRM.EmployeeQuery filter)
    {
        return await employeeService.GetEmployeesAsync(filter);
    }

    public async Task<bool> IsEmployeeCodeUnique(
        [Service] IEmployeeService employeeService,
        string employeeCode,
        Guid? excludeEmployeeId = null)
    {
        return await employeeService.IsEmployeeCodeUniqueAsync(employeeCode, excludeEmployeeId);
    }
}