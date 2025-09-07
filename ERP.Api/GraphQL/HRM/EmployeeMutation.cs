using ERP.Application.HRM;
using ERP.Shared.DTOs.HRM;
using HotChocolate;

namespace ERP.Api.GraphQL.HRM;

[ExtendObjectType("Mutation")]
public class EmployeeMutation
{
    public async Task<EmployeeDto> AddEmployee(
        [Service] IEmployeeService employeeService,
        CreateEmployeeDto input)
    {
        return await employeeService.AddEmployeeAsync(input);
    }

    public async Task<EmployeeDto> UpdateEmployee(
        [Service] IEmployeeService employeeService,
        UpdateEmployeeDto input)
    {
        return await employeeService.UpdateEmployeeAsync(input);
    }

    public async Task<bool> DeleteEmployee(
        [Service] IEmployeeService employeeService,
        Guid employeeId)
    {
        await employeeService.DeleteEmployeeAsync(employeeId);
        return true;
    }
}