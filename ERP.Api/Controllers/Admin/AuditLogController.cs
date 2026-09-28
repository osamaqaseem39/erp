using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Admin;
using ERP.Shared.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/audit-logs")]
[Authorize]
public class AuditLogController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IAuditLogService _service;

    public AuditLogController(IAuditLogService service, ILogger<AuditLogController> logger) : base(logger)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("entity/{entityName}/{entityId}")]
    public async Task<IActionResult> GetByEntity(string entityName, string entityId)
    {
        var result = await _service.GetByEntityAsync(entityName, entityId);
        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(Guid userId)
    {
        var result = await _service.GetByUserAsync(userId);
        return Ok(result);
    }

    [HttpGet("action/{action}")]
    public async Task<IActionResult> GetByAction(string action)
    {
        var result = await _service.GetByActionAsync(action);
        return Ok(result);
    }

    [HttpGet("date-range")]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _service.GetByDateRangeAsync(startDate, endDate);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAuditLogDto dto)
    {
        var id = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPost("log-action")]
    public async Task<IActionResult> LogAction([FromBody] LogActionDto dto)
    {
        var result = await _service.LogActionAsync(
            dto.EntityName, 
            dto.EntityId, 
            dto.Action, 
            dto.OldValues, 
            dto.NewValues, 
            dto.UserId);
        return result ? Ok() : BadRequest();
    }
}

// DTO for logging actions
public class LogActionDto
{
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public Guid? UserId { get; set; }
}