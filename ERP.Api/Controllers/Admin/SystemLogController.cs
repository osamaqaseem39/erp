using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Admin;
using ERP.Shared.DTOs.Admin;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/system-logs")]
public class SystemLogController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ISystemLogService _service;

    public SystemLogController(ISystemLogService service, ILogger<SystemLogController> logger) : base(logger)
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

    [HttpGet("level/{logLevel}")]
    public async Task<IActionResult> GetByLevel(string logLevel)
    {
        try
        {
            var result = await _service.GetByLevelAsync(logLevel);
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
    public async Task<IActionResult> Create([FromBody] CreateSystemLogDto dto)
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSystemLogDto dto)
    {
        try
        {
            dto.Id = id;
            var result = await _service.UpdateAsync(dto);
            return result ? Ok() : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            return result ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}