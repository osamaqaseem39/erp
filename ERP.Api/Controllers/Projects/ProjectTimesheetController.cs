using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Projects;
using ERP.Shared.DTOs.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Projects;

[ApiController]
[Route("api/projects/timesheets")]
[Authorize]
[PermissionResource("projects.timesheets")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class ProjectTimesheetController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IProjectTimesheetService _service;

    public ProjectTimesheetController(IProjectTimesheetService service, ILogger<ProjectTimesheetController> logger) : base(logger) =>
        _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProjectTimesheetDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> List([FromQuery] Guid? projectId = null) =>
        Ok(await _service.ListAsync(HttpContext.RequireTenantId(), projectId));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProjectTimesheetDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Get(Guid id)
    {
        var row = await _service.GetAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProjectTimesheetDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateProjectTimesheetDto dto)
    {
        var result = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(typeof(ProjectTimesheetDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Submit(Guid id) =>
        Ok(await _service.SubmitAsync(id, HttpContext.RequireTenantId(), User.RequireUserId()));

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(ProjectTimesheetDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Approve(Guid id) =>
        Ok(await _service.ApproveAsync(id, HttpContext.RequireTenantId(), User.RequireUserId()));
}
