using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Projects;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Projects;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Projects;

[ApiController]
[Route("api/projects/documents")]
[Authorize]
[PermissionResource("projects.projects")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class ProjectDocumentController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IProjectDocumentService _service;

    public ProjectDocumentController(IProjectDocumentService service, ILogger<ProjectDocumentController> logger)
        : base(logger) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProjectDocumentDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? projectId = null, [FromQuery] string? documentType = null)
    {
        var tenantId = GetTenantId();
        if (projectId.HasValue)
            return Ok(await _service.GetByProjectAsync(projectId.Value, tenantId));
        if (!string.IsNullOrWhiteSpace(documentType))
            return Ok(await _service.GetByTypeAsync(documentType, tenantId));
        return Ok(await _service.GetAllAsync(tenantId));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDocumentDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var row = await _service.GetByIdAsync(id, GetTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost]
    [ProducesResponseType(typeof(IdResponse), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateProjectDocumentDto dto)
    {
        dto.TenantId = GetTenantId();
        dto.CreatedBy = GetUserId().ToString();
        var id = await _service.CreateAsync(dto);
        if (id == Guid.Empty) return BadRequest(new { error = "Invalid document or project." });
        return CreatedAtAction(nameof(GetById), new { id }, new IdResponse { Id = id });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDocumentDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectDocumentDto dto)
    {
        dto.Id = id;
        dto.UpdatedBy = GetUserId().ToString();
        var ok = await _service.UpdateAsync(dto, GetTenantId());
        return ok ? Ok(await _service.GetByIdAsync(id, GetTenantId())) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteAsync(id, GetTenantId());
        return ok ? NoContent() : NotFound();
    }

    [HttpGet("{id:guid}/download")]
    [ProducesResponseType(typeof(FileContentResult), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Download(Guid id)
    {
        var payload = await _service.DownloadAsync(id, GetTenantId());
        if (payload == null) return NotFound(new { error = "No file attached to this document." });
        return File(payload.Content, payload.ContentType, payload.FileName);
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
