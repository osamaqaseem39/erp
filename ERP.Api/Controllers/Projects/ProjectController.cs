using ERP.Api.Security.Permissions;
using ERP.Api.Security;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Projects;
using ERP.Shared.DTOs.Projects;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Projects;

[ApiController]
[Route("api/projects")]
[Authorize]
[PermissionResource("projects.projects")]
public class ProjectController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ILogger<ProjectController> _logger;

    public ProjectController(IProjectService projectService, ILogger<ProjectController> logger) : base(logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _projectService.CreateAsync(dto, tenantId, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.ProjectId }, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = GetTenantId();
        var project = await _projectService.GetByIdAsync(id, tenantId);
        if (project == null)
            return NotFound();
        return Ok(project);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null, [FromQuery] Guid? managerId = null, [FromQuery] Guid? customerId = null)
    {
        var tenantId = GetTenantId();
        var projects = await _projectService.GetAllAsync(tenantId, status, managerId, customerId);
        return Ok(projects);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectDto dto)
    {
        if (id != dto.Id)
            return BadRequest("ID mismatch");

        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _projectService.UpdateAsync(dto, tenantId, userId);
        return Ok(result);
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> Start(Guid id)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _projectService.StartAsync(id, tenantId, userId);
        return Ok(result);
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteProjectDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _projectService.CompleteAsync(id, dto, tenantId, userId);
        return Ok(result);
    }

    [HttpPost("{id}/pause")]
    public async Task<IActionResult> Pause(Guid id, [FromBody] PauseProjectDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _projectService.PauseAsync(id, dto, tenantId, userId);
        return Ok(result);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelProjectDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _projectService.CancelAsync(id, dto, tenantId, userId);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _projectService.DeleteAsync(id, tenantId);
        if (!result)
            return NotFound();
        return NoContent();
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();

    private Guid GetUserId() => User.RequireUserId();
}