using ERP.Api.Security;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Admin;
using ERP.Shared.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/system-settings")]
// Raw key/value "advanced settings": SuperAdmin only. Tenant admins use /api/core/settings/{category}.
[Authorize(Roles = "SuperAdmin,SUPER_ADMIN")]
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
        var tenantId = GetTenantId();
        var result = await _service.GetByIdAsync(id, tenantId);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("key/{key}")]
    public async Task<IActionResult> GetByKey(string key)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetByKeyAsync(key, tenantId);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category = null)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetAllAsync(tenantId, category);
        return Ok(result);
    }

    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetByCategory(string category)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetByCategoryAsync(category, tenantId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSystemSettingDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _service.CreateAsync(dto, tenantId, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSystemSettingDto dto)
    {
        dto.Id = id;
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _service.UpdateAsync(dto, tenantId, userId);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _service.DeleteAsync(id, tenantId);
        return result ? NoContent() : NotFound();
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert([FromBody] UpsertSystemSettingDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _service.UpsertAsync(dto, tenantId, userId);
        return Ok(result);
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();

    private Guid GetUserId() => User.RequireUserId();
}