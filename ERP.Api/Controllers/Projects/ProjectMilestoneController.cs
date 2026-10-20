using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Projects;
using ERP.Shared.DTOs.Projects;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Projects;

[ApiController]
[Route("api/projects/milestones")]
[Authorize]
[PermissionResource("projects.projects")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class ProjectMilestoneController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IProjectMilestoneService _service;

    public ProjectMilestoneController(IProjectMilestoneService service, ILogger<ProjectMilestoneController> logger) : base(logger)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProjectMilestoneDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? projectId = null, [FromQuery] string? status = null)
    {
        var tenantId = GetTenantId();
        if (projectId.HasValue)
            return Ok(await _service.GetByProjectAsync(projectId.Value, tenantId));
        if (!string.IsNullOrWhiteSpace(status))
            return Ok(await _service.GetByStatusAsync(status, tenantId));
        return Ok(await _service.GetAllAsync(tenantId));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProjectMilestoneDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id, GetTenantId());
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateProjectMilestoneDto dto)
    {
        var id = await _service.CreateAsync(dto, GetTenantId(), GetUserId());
        if (id == Guid.Empty) return BadRequest(new { error = "Invalid milestone." });
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectMilestoneDto dto)
    {
        dto.Id = id;
        var ok = await _service.UpdateAsync(dto, GetTenantId(), GetUserId());
        return ok ? Ok() : NotFound();
    }

    [HttpPost("{id}/complete")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Complete(Guid id)
    {
        var ok = await _service.CompleteAsync(id, GetTenantId(), GetUserId());
        return ok ? Ok() : NotFound();
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
