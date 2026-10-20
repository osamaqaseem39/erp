using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Application.Interfaces.HRM;
using ERP.Shared.DTOs.HRM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/leave-types")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class LeaveTypeController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ILeaveTypeService _service;

    public LeaveTypeController(ILeaveTypeService service, ILogger<LeaveTypeController> logger) : base(logger) =>
        _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LeaveTypeDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(HttpContext.RequireTenantId()));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LeaveTypeDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var row = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost]
    [ProducesResponseType(typeof(LeaveTypeDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateLeaveTypeDto dto)
    {
        var row = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return CreatedAtAction(nameof(GetById), new { id = row.Id }, row);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(LeaveTypeDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeaveTypeDto dto)
    {
        if (id != dto.Id) return BadRequest("ID mismatch");
        var row = await _service.UpdateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return Ok(row);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteAsync(id, HttpContext.RequireTenantId());
        return ok ? NoContent() : NotFound();
    }
}
