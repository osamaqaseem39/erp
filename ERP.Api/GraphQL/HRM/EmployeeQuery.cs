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
        Guid employeeId,
        Guid tenantId)
    {
        try
        {
            return await employeeService.GetEmployeeByIdAsync(employeeId, tenantId);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    public async Task<EmployeeDto?> GetEmployeeByCode(
        [Service] IEmployeeService employeeService,
        string employeeCode,
        Guid tenantId)
    {
        try
        {
            return await employeeService.GetEmployeeByCodeAsync(employeeCode, tenantId);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    public async Task<PagedResult<EmployeeListDto>> GetAllEmployees(
        [Service] IEmployeeService employeeService,
        ERP.Shared.DTOs.HRM.EmployeeQuery filter,
        Guid tenantId)
    {
        return await employeeService.GetEmployeesAsync(filter, tenantId);
    }

    public async Task<bool> IsEmployeeCodeUnique(
        [Service] IEmployeeService employeeService,
        string employeeCode,
        Guid tenantId,
        Guid? excludeEmployeeId = null)
    {
        return await employeeService.IsEmployeeCodeUniqueAsync(employeeCode, tenantId, excludeEmployeeId);
    }
}
