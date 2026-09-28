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
        try
        {
            var result = await _service.GetByIdAsync(id);
            return result != null ? Ok(result) : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("entity/{entityName}/{entityId}")]
    public async Task<IActionResult> GetByEntity(string entityName, string entityId)
    {
        try
        {
            var result = await _service.GetByEntityAsync(entityName, entityId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(Guid userId)
    {
        try
        {
            var result = await _service.GetByUserAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("action/{action}")]
    public async Task<IActionResult> GetByAction(string action)
    {
        try
        {
            var result = await _service.GetByActionAsync(action);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("date-range")]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var result = await _service.GetByDateRangeAsync(startDate, endDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAuditLogDto dto)
    {
        try
        {
            var id = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("log-action")]
    public async Task<IActionResult> LogAction([FromBody] LogActionDto dto)
    {
        try
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
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
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