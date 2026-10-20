using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.HRM;
using ERP.Shared.DTOs.HRM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/employee-advances")]
[Authorize]
[PermissionResource("hrm.employeeAdvances")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class EmployeeAdvanceController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IEmployeeAdvanceService _service;

    public EmployeeAdvanceController(IEmployeeAdvanceService service, ILogger<EmployeeAdvanceController> logger) : base(logger) =>
        _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmployeeAdvanceDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> List([FromQuery] Guid? employeeId = null) =>
        Ok(await _service.ListAsync(HttpContext.RequireTenantId(), employeeId));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeAdvanceDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Get(Guid id)
    {
        var row = await _service.GetAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost]
    [ProducesResponseType(typeof(EmployeeAdvanceDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Disburse([FromBody] CreateEmployeeAdvanceDto dto)
    {
        var result = await _service.DisburseAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/recover")]
    [ProducesResponseType(typeof(EmployeeAdvanceDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Recover(Guid id, [FromBody] RecoverEmployeeAdvanceDto dto) =>
        Ok(await _service.RecoverAsync(id, dto, HttpContext.RequireTenantId(), User.RequireUserId()));
}
