using ERP.Application.HRM;
using ERP.Shared.DTOs.HRM;
using HotChocolate;

namespace ERP.Api.GraphQL.HRM;

[ExtendObjectType("Mutation")]
public class EmployeeMutation
{
    public async Task<EmployeeDto> AddEmployee(
        [Service] IEmployeeService employeeService,
        CreateEmployeeDto input,
        Guid tenantId,
        Guid userId)
    {
        return await employeeService.AddEmployeeAsync(input, tenantId, userId);
    }

    public async Task<EmployeeDto> UpdateEmployee(
        [Service] IEmployeeService employeeService,
        UpdateEmployeeDto input,
        Guid tenantId,
        Guid userId)
    {
        return await employeeService.UpdateEmployeeAsync(input, tenantId, userId);
    }

    public async Task<bool> DeleteEmployee(
        [Service] IEmployeeService employeeService,
        Guid employeeId,
        Guid tenantId)
    {
        await employeeService.DeleteEmployeeAsync(employeeId, tenantId);
        return true;
    }
}
