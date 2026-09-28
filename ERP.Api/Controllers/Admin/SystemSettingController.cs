using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Admin;
using ERP.Shared.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/system-settings")]
[Authorize]
public class SystemSettingController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ISystemSettingService _service;

    public SystemSettingController(ISystemSettingService service, ILogger<SystemSettingController> logger) : base(logger)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _service.GetByIdAsync(id, tenantId);
            return result != null ? Ok(result) : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("key/{key}")]
    public async Task<IActionResult> GetByKey(string key)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _service.GetByKeyAsync(key, tenantId);
            return result != null ? Ok(result) : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category = null)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _service.GetAllAsync(tenantId, category);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetByCategory(string category)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _service.GetByCategoryAsync(category, tenantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSystemSettingDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.CreateAsync(dto, tenantId, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSystemSettingDto dto)
    {
        try
        {
            dto.Id = id;
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.UpdateAsync(dto, tenantId, userId);
            return Ok(result);
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
            var tenantId = GetTenantId();
            var result = await _service.DeleteAsync(id, tenantId);
            return result ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert([FromBody] UpsertSystemSettingDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.UpsertAsync(dto, tenantId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    private Guid GetTenantId()
    {
        // Implementation to get tenant ID from context
        return Guid.NewGuid(); // Placeholder
    }

    private Guid GetUserId()
    {
        // Implementation to get user ID from context
        return Guid.NewGuid(); // Placeholder
    }
}