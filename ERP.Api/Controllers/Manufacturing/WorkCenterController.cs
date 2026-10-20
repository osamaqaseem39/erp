using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Manufacturing;

[ApiController]
[Route("api/manufacturing/work-centers")]
[Authorize]
[PermissionResource("manufacturing.workCenters")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class WorkCenterController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IWorkCenterService _service;

    public WorkCenterController(IWorkCenterService service, ILogger<WorkCenterController> logger) : base(logger)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WorkCenterDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null)
    {
        var result = await _service.GetAllAsync(GetTenantId(), isActive);
        return Ok(result);
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<WorkCenterDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetActive()
    {
        var result = await _service.GetActiveAsync(GetTenantId());
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(WorkCenterDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id, GetTenantId());
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(typeof(WorkCenterDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateWorkCenterDto dto)
    {
        var result = await _service.CreateAsync(dto, GetTenantId(), GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(WorkCenterDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWorkCenterDto dto)
    {
        dto.Id = id;
        var result = await _service.UpdateAsync(dto, GetTenantId(), GetUserId());
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteAsync(id, GetTenantId());
        return ok ? NoContent() : NotFound();
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
