using ERP.Api.Abstractions;
using ERP.Application.HRM;
using ERP.Shared.DTOs.HRM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/employees")]
[Authorize]
public class EmployeeController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger) : base(logger)
    {
        _employeeService = employeeService;
    }

    [HttpPost("addEmployee")]
    public async Task<IActionResult> AddEmployee([FromBody] CreateEmployeeDto dto)
    {
        var result = await _employeeService.AddEmployeeAsync(dto);
        return Ok(result);
    }

    [HttpPut("updateEmployee")]
    public async Task<IActionResult> UpdateEmployee([FromBody] UpdateEmployeeDto dto)
    {
        var result = await _employeeService.UpdateEmployeeAsync(dto);
        return Ok(result);
    }

    [HttpDelete("deleteEmployee")]
    public async Task<IActionResult> DeleteEmployee([FromQuery] Guid employeeId)
    {
        await _employeeService.DeleteEmployeeAsync(employeeId);
        return Ok(new { message = "Employee deleted successfully" });
    }

    [HttpGet("getEmployeeById")]
    public async Task<IActionResult> GetEmployeeById([FromQuery] Guid employeeId)
    {
        var result = await _employeeService.GetEmployeeByIdAsync(employeeId);
        return Ok(result);
    }

    [HttpGet("getAllEmployees")]
    public async Task<IActionResult> GetAllEmployees([FromQuery] EmployeeQuery filter)
    {
        var result = await _employeeService.GetEmployeesAsync(filter);
        return Ok(result);
    }

    [HttpGet("getEmployeeByCode")]
    public async Task<IActionResult> GetEmployeeByCode([FromQuery] string employeeCode)
    {
        var result = await _employeeService.GetEmployeeByCodeAsync(employeeCode);
        return Ok(result);
    }

    [HttpGet("isEmployeeCodeUnique")]
    public async Task<IActionResult> IsEmployeeCodeUnique([FromQuery] string employeeCode, [FromQuery] Guid? excludeEmployeeId = null)
    {
        var result = await _employeeService.IsEmployeeCodeUniqueAsync(employeeCode, excludeEmployeeId);
        return Ok(new { isUnique = result });
    }
}