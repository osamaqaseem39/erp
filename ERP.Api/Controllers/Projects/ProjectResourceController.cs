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
[Route("api/projects/resources")]
[Authorize]
[PermissionResource("projects.projects")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class ProjectResourceController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IProjectResourceService _service;

    public ProjectResourceController(IProjectResourceService service, ILogger<ProjectResourceController> logger) : base(logger)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProjectResourceDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? projectId = null, [FromQuery] Guid? employeeId = null)
    {
        var tenantId = GetTenantId();
        if (projectId.HasValue)
            return Ok(await _service.GetByProjectAsync(projectId.Value, tenantId));
        if (employeeId.HasValue)
            return Ok(await _service.GetByEmployeeAsync(employeeId.Value, tenantId));
        return Ok(await _service.GetAllAsync(tenantId));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProjectResourceDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id, GetTenantId());
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateProjectResourceDto dto)
    {
        var id = await _service.CreateAsync(dto, GetTenantId(), GetUserId());
        if (id == Guid.Empty) return BadRequest(new { error = "Invalid resource." });
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectResourceDto dto)
    {
        dto.Id = id;
        var ok = await _service.UpdateAsync(dto, GetTenantId(), GetUserId());
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
