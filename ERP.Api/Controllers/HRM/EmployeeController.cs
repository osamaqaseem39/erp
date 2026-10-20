using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Api.Security;
using ERP.Application.HRM;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.HRM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/employees")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class EmployeeController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger) : base(logger)
    {
        _employeeService = employeeService;
    }

    [HttpPost("addEmployee")]
    [ProducesResponseType(typeof(EmployeeDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> AddEmployee([FromBody] CreateEmployeeDto dto)
    {
        var result = await _employeeService.AddEmployeeAsync(dto, GetTenantId(), GetUserId());
        return Ok(result);
    }

    [HttpPut("updateEmployee")]
    [ProducesResponseType(typeof(EmployeeDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> UpdateEmployee([FromBody] UpdateEmployeeDto dto)
    {
        var result = await _employeeService.UpdateEmployeeAsync(dto, GetTenantId(), GetUserId());
        return Ok(result);
    }

    [HttpDelete("deleteEmployee")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> DeleteEmployee([FromQuery] Guid employeeId)
    {
        await _employeeService.DeleteEmployeeAsync(employeeId, GetTenantId());
        return Ok(new MessageResponse { Message = "Employee deleted successfully" });
    }

    [HttpGet("getEmployeeById")]
    [ProducesResponseType(typeof(EmployeeDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetEmployeeById([FromQuery] Guid employeeId)
    {
        var result = await _employeeService.GetEmployeeByIdAsync(employeeId, GetTenantId());
        return Ok(result);
    }

    [HttpGet("getAllEmployees")]
    [ProducesResponseType(typeof(PagedResult<EmployeeListDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAllEmployees([FromQuery] EmployeeQuery filter)
    {
        var result = await _employeeService.GetEmployeesAsync(filter, GetTenantId());
        return Ok(result);
    }

    [HttpGet("getEmployeeByCode")]
    [ProducesResponseType(typeof(EmployeeDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetEmployeeByCode([FromQuery] string employeeCode)
    {
        var result = await _employeeService.GetEmployeeByCodeAsync(employeeCode, GetTenantId());
        return Ok(result);
    }

    [HttpGet("isEmployeeCodeUnique")]
    [ProducesResponseType(typeof(SuccessResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> IsEmployeeCodeUnique([FromQuery] string employeeCode, [FromQuery] Guid? excludeEmployeeId = null)
    {
        var result = await _employeeService.IsEmployeeCodeUniqueAsync(employeeCode, GetTenantId(), excludeEmployeeId);
        return Ok(new SuccessResponse { Success = result });
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
