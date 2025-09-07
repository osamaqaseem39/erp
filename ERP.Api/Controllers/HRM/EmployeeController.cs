using ERP.Api.Abstractions;
using ERP.Application.HRM;
using ERP.Shared.DTOs.HRM;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/employees")]
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
        try
        {
            var result = await _employeeService.AddEmployeeAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("updateEmployee")]
    public async Task<IActionResult> UpdateEmployee([FromBody] UpdateEmployeeDto dto)
    {
        try
        {
            var result = await _employeeService.UpdateEmployeeAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("deleteEmployee")]
    public async Task<IActionResult> DeleteEmployee([FromQuery] Guid employeeId)
    {
        try
        {
            await _employeeService.DeleteEmployeeAsync(employeeId);
            return Ok(new { message = "Employee deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("getEmployeeById")]
    public async Task<IActionResult> GetEmployeeById([FromQuery] Guid employeeId)
    {
        try
        {
            var result = await _employeeService.GetEmployeeByIdAsync(employeeId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("getAllEmployees")]
    public async Task<IActionResult> GetAllEmployees([FromQuery] EmployeeQuery filter)
    {
        try
        {
            var result = await _employeeService.GetEmployeesAsync(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("getEmployeeByCode")]
    public async Task<IActionResult> GetEmployeeByCode([FromQuery] string employeeCode)
    {
        try
        {
            var result = await _employeeService.GetEmployeeByCodeAsync(employeeCode);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("isEmployeeCodeUnique")]
    public async Task<IActionResult> IsEmployeeCodeUnique([FromQuery] string employeeCode, [FromQuery] Guid? excludeEmployeeId = null)
    {
        try
        {
            var result = await _employeeService.IsEmployeeCodeUniqueAsync(employeeCode, excludeEmployeeId);
            return Ok(new { isUnique = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}