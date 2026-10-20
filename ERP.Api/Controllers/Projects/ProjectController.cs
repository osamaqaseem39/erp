using ERP.Api.Security.Permissions;
using ERP.Api.Security;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Projects;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Projects;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Projects;

[ApiController]
[Route("api/projects")]
[Authorize]
[PermissionResource("projects.projects")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class ProjectController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly IProjectTaskService _tasks;
    private readonly IProjectMilestoneService _milestones;
    private readonly IProjectResourceService _resources;
    private readonly IProjectDocumentService _documents;

    public ProjectController(
        IProjectService projectService,
        IProjectTaskService tasks,
        IProjectMilestoneService milestones,
        IProjectResourceService resources,
        IProjectDocumentService documents,
        ILogger<ProjectController> logger) : base(logger)
    {
        _projectService = projectService;
        _tasks = tasks;
        _milestones = milestones;
        _resources = resources;
        _documents = documents;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProjectCreateResult), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
    {
        var result = await _projectService.CreateAsync(dto, GetTenantId(), GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = result.ProjectId }, result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProjectDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var project = await _projectService.GetByIdAsync(id, GetTenantId());
        return project == null ? NotFound() : Ok(project);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProjectDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null, [FromQuery] Guid? managerId = null, [FromQuery] Guid? customerId = null)
        => Ok(await _projectService.GetAllAsync(GetTenantId(), status, managerId, customerId));

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SuccessResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectDto dto)
    {
        if (id != dto.Id) return BadRequest("ID mismatch");
        var result = await _projectService.UpdateAsync(dto, GetTenantId(), GetUserId());
        return Ok(new SuccessResponse { Success = result });
    }

    [HttpPost("{id}/start")]
    [ProducesResponseType(typeof(SuccessResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Start(Guid id)
        => Ok(new SuccessResponse { Success = await _projectService.StartAsync(id, GetTenantId(), GetUserId()) });

    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(SuccessResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteProjectDto dto)
        => Ok(new SuccessResponse { Success = await _projectService.CompleteAsync(id, dto, GetTenantId(), GetUserId()) });

    [HttpPost("{id}/pause")]
    [ProducesResponseType(typeof(SuccessResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Pause(Guid id, [FromBody] PauseProjectDto dto)
        => Ok(new SuccessResponse { Success = await _projectService.PauseAsync(id, dto, GetTenantId(), GetUserId()) });

    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(SuccessResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelProjectDto dto)
        => Ok(new SuccessResponse { Success = await _projectService.CancelAsync(id, dto, GetTenantId(), GetUserId()) });

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _projectService.DeleteAsync(id, GetTenantId());
        return result ? NoContent() : NotFound();
    }

    [HttpGet("{projectId:guid}/tasks")]
    [ProducesResponseType(typeof(IEnumerable<ProjectTaskDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ListTasks(Guid projectId)
        => Ok(await _tasks.GetByProjectAsync(projectId, GetTenantId()));

    [HttpGet("{projectId:guid}/tasks/{taskId:guid}")]
    [ProducesResponseType(typeof(ProjectTaskDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetTask(Guid projectId, Guid taskId)
    {
        var row = await _tasks.GetByIdAsync(taskId, GetTenantId());
        if (row == null || row.ProjectId != projectId) return NotFound();
        return Ok(row);
    }

    [HttpPost("{projectId:guid}/tasks")]
    [ProducesResponseType(typeof(IdResponse), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateTask(Guid projectId, [FromBody] CreateProjectTaskDto dto)
    {
        dto.ProjectId = projectId;
        var id = await _tasks.CreateAsync(dto, GetTenantId(), GetUserId());
        if (id == Guid.Empty) return BadRequest(new { error = "Could not create task." });
        return CreatedAtAction(nameof(GetTask), new { projectId, taskId = id }, new IdResponse { Id = id });
    }

    [HttpPut("{projectId:guid}/tasks/{taskId:guid}")]
    [ProducesResponseType(typeof(ProjectTaskDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> UpdateTask(Guid projectId, Guid taskId, [FromBody] UpdateProjectTaskDto dto)
    {
        dto.Id = taskId;
        var existing = await _tasks.GetByIdAsync(taskId, GetTenantId());
        if (existing == null || existing.ProjectId != projectId) return NotFound();
        var ok = await _tasks.UpdateAsync(dto, GetTenantId(), GetUserId());
        return ok ? Ok(await _tasks.GetByIdAsync(taskId, GetTenantId())) : BadRequest(new { error = "Could not update task." });
    }

    [HttpPost("{projectId:guid}/tasks/{taskId:guid}/complete")]
    [ProducesResponseType(typeof(ProjectTaskDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> CompleteTask(Guid projectId, Guid taskId)
    {
        var existing = await _tasks.GetByIdAsync(taskId, GetTenantId());
        if (existing == null || existing.ProjectId != projectId) return NotFound();
        var ok = await _tasks.CompleteAsync(taskId, GetTenantId(), GetUserId());
        return ok ? Ok(await _tasks.GetByIdAsync(taskId, GetTenantId())) : BadRequest(new { error = "Could not complete task." });
    }

    [HttpDelete("{projectId:guid}/tasks/{taskId:guid}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> DeleteTask(Guid projectId, Guid taskId)
    {
        var existing = await _tasks.GetByIdAsync(taskId, GetTenantId());
        if (existing == null || existing.ProjectId != projectId) return NotFound();
        var ok = await _tasks.DeleteAsync(taskId, GetTenantId());
        return ok ? NoContent() : BadRequest(new { error = "Could not delete task." });
    }

    [HttpGet("{projectId:guid}/milestones")]
    [ProducesResponseType(typeof(IEnumerable<ProjectMilestoneDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ListMilestones(Guid projectId)
        => Ok(await _milestones.GetByProjectAsync(projectId, GetTenantId()));

    [HttpGet("{projectId:guid}/milestones/{milestoneId:guid}")]
    [ProducesResponseType(typeof(ProjectMilestoneDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetMilestone(Guid projectId, Guid milestoneId)
    {
        var row = await _milestones.GetByIdAsync(milestoneId, GetTenantId());
        if (row == null || row.ProjectId != projectId) return NotFound();
        return Ok(row);
    }

    [HttpPost("{projectId:guid}/milestones")]
    [ProducesResponseType(typeof(IdResponse), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateMilestone(Guid projectId, [FromBody] CreateProjectMilestoneDto dto)
    {
        dto.ProjectId = projectId;
        var id = await _milestones.CreateAsync(dto, GetTenantId(), GetUserId());
        if (id == Guid.Empty) return BadRequest(new { error = "Could not create milestone." });
        return CreatedAtAction(nameof(GetMilestone), new { projectId, milestoneId = id }, new IdResponse { Id = id });
    }

    [HttpPut("{projectId:guid}/milestones/{milestoneId:guid}")]
    [ProducesResponseType(typeof(ProjectMilestoneDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> UpdateMilestone(Guid projectId, Guid milestoneId, [FromBody] UpdateProjectMilestoneDto dto)
    {
        dto.Id = milestoneId;
        var existing = await _milestones.GetByIdAsync(milestoneId, GetTenantId());
        if (existing == null || existing.ProjectId != projectId) return NotFound();
        var ok = await _milestones.UpdateAsync(dto, GetTenantId(), GetUserId());
        return ok ? Ok(await _milestones.GetByIdAsync(milestoneId, GetTenantId())) : BadRequest(new { error = "Could not update milestone." });
    }

    [HttpPost("{projectId:guid}/milestones/{milestoneId:guid}/complete")]
    [ProducesResponseType(typeof(ProjectMilestoneDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> CompleteMilestone(Guid projectId, Guid milestoneId)
    {
        var existing = await _milestones.GetByIdAsync(milestoneId, GetTenantId());
        if (existing == null || existing.ProjectId != projectId) return NotFound();
        var ok = await _milestones.CompleteAsync(milestoneId, GetTenantId(), GetUserId());
        return ok ? Ok(await _milestones.GetByIdAsync(milestoneId, GetTenantId())) : BadRequest(new { error = "Could not complete milestone." });
    }

    [HttpDelete("{projectId:guid}/milestones/{milestoneId:guid}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> DeleteMilestone(Guid projectId, Guid milestoneId)
    {
        var existing = await _milestones.GetByIdAsync(milestoneId, GetTenantId());
        if (existing == null || existing.ProjectId != projectId) return NotFound();
        var ok = await _milestones.DeleteAsync(milestoneId, GetTenantId());
        return ok ? NoContent() : BadRequest(new { error = "Could not delete milestone." });
    }

    [HttpGet("{projectId:guid}/resources")]
    [ProducesResponseType(typeof(IEnumerable<ProjectResourceDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ListResources(Guid projectId)
        => Ok(await _resources.GetByProjectAsync(projectId, GetTenantId()));

    [HttpGet("{projectId:guid}/resources/{resourceId:guid}")]
    [ProducesResponseType(typeof(ProjectResourceDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetResource(Guid projectId, Guid resourceId)
    {
        var row = await _resources.GetByIdAsync(resourceId, GetTenantId());
        if (row == null || row.ProjectId != projectId) return NotFound();
        return Ok(row);
    }

    [HttpPost("{projectId:guid}/resources")]
    [ProducesResponseType(typeof(IdResponse), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateResource(Guid projectId, [FromBody] CreateProjectResourceDto dto)
    {
        dto.ProjectId = projectId;
        var id = await _resources.CreateAsync(dto, GetTenantId(), GetUserId());
        if (id == Guid.Empty) return BadRequest(new { error = "Could not create resource." });
        return CreatedAtAction(nameof(GetResource), new { projectId, resourceId = id }, new IdResponse { Id = id });
    }

    [HttpPut("{projectId:guid}/resources/{resourceId:guid}")]
    [ProducesResponseType(typeof(ProjectResourceDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> UpdateResource(Guid projectId, Guid resourceId, [FromBody] UpdateProjectResourceDto dto)
    {
        dto.Id = resourceId;
        var existing = await _resources.GetByIdAsync(resourceId, GetTenantId());
        if (existing == null || existing.ProjectId != projectId) return NotFound();
        var ok = await _resources.UpdateAsync(dto, GetTenantId(), GetUserId());
        return ok ? Ok(await _resources.GetByIdAsync(resourceId, GetTenantId())) : BadRequest(new { error = "Could not update resource." });
    }

    [HttpDelete("{projectId:guid}/resources/{resourceId:guid}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> DeleteResource(Guid projectId, Guid resourceId)
    {
        var existing = await _resources.GetByIdAsync(resourceId, GetTenantId());
        if (existing == null || existing.ProjectId != projectId) return NotFound();
        var ok = await _resources.DeleteAsync(resourceId, GetTenantId());
        return ok ? NoContent() : BadRequest(new { error = "Could not delete resource." });
    }

    [HttpGet("{projectId:guid}/documents")]
    [ProducesResponseType(typeof(IEnumerable<ProjectDocumentDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ListDocuments(Guid projectId)
        => Ok(await _documents.GetByProjectAsync(projectId, GetTenantId()));

    [HttpGet("{projectId:guid}/documents/{documentId:guid}")]
    [ProducesResponseType(typeof(ProjectDocumentDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetDocument(Guid projectId, Guid documentId)
    {
        var row = await _documents.GetByIdAsync(documentId, GetTenantId());
        if (row == null || row.ProjectId != projectId) return NotFound();
        return Ok(row);
    }

    [HttpPost("{projectId:guid}/documents")]
    [ProducesResponseType(typeof(IdResponse), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateDocument(Guid projectId, [FromBody] CreateProjectDocumentDto dto)
    {
        dto.ProjectId = projectId;
        dto.TenantId = GetTenantId();
        dto.CreatedBy = GetUserId().ToString();
        var id = await _documents.CreateAsync(dto);
        if (id == Guid.Empty) return BadRequest(new { error = "Could not create document." });
        return CreatedAtAction(nameof(GetDocument), new { projectId, documentId = id }, new IdResponse { Id = id });
    }

    [HttpPut("{projectId:guid}/documents/{documentId:guid}")]
    [ProducesResponseType(typeof(ProjectDocumentDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> UpdateDocument(Guid projectId, Guid documentId, [FromBody] UpdateProjectDocumentDto dto)
    {
        dto.Id = documentId;
        dto.UpdatedBy = GetUserId().ToString();
        var existing = await _documents.GetByIdAsync(documentId, GetTenantId());
        if (existing == null || existing.ProjectId != projectId) return NotFound();
        var ok = await _documents.UpdateAsync(dto, GetTenantId());
        return ok ? Ok(await _documents.GetByIdAsync(documentId, GetTenantId())) : BadRequest(new { error = "Could not update document." });
    }

    [HttpPost("{projectId:guid}/documents/{documentId:guid}/upload")]
    [ProducesResponseType(typeof(SuccessResponse), OpenApiResponseTypes.Ok)]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> UploadDocument(Guid projectId, Guid documentId, IFormFile file)
    {
        var existing = await _documents.GetByIdAsync(documentId, GetTenantId());
        if (existing == null || existing.ProjectId != projectId) return NotFound();
        if (file == null || file.Length == 0) return BadRequest(new { error = "File is required." });

        await using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var ok = await _documents.UploadAsync(documentId, GetTenantId(), file.FileName, ms.ToArray(), GetUserId().ToString());
        return ok ? Ok(new SuccessResponse { Success = true }) : BadRequest(new { error = "Upload failed." });
    }

    [HttpGet("{projectId:guid}/documents/{documentId:guid}/download")]
    [ProducesResponseType(typeof(FileContentResult), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> DownloadDocument(Guid projectId, Guid documentId)
    {
        var existing = await _documents.GetByIdAsync(documentId, GetTenantId());
        if (existing == null || existing.ProjectId != projectId) return NotFound();
        var payload = await _documents.DownloadAsync(documentId, GetTenantId());
        if (payload == null) return NotFound(new { error = "No file attached to this document." });
        return File(payload.Content, payload.ContentType, payload.FileName);
    }

    [HttpDelete("{projectId:guid}/documents/{documentId:guid}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> DeleteDocument(Guid projectId, Guid documentId)
    {
        var existing = await _documents.GetByIdAsync(documentId, GetTenantId());
        if (existing == null || existing.ProjectId != projectId) return NotFound();
        var ok = await _documents.DeleteAsync(documentId, GetTenantId());
        return ok ? NoContent() : BadRequest(new { error = "Could not delete document." });
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
