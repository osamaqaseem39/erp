using ERP.Api.Security;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Admin;
using ERP.Shared.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/roles")]
[Authorize]
public class RoleController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RoleController> _logger;

    public RoleController(IRoleService roleService, ILogger<RoleController> logger) : base(logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _roleService.CreateAsync(dto, tenantId, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = GetTenantId();
        var role = await _roleService.GetByIdAsync(id, tenantId);
        if (role == null)
            return NotFound();
        return Ok(role);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null)
    {
        var tenantId = GetTenantId();
        var roles = await _roleService.GetAllAsync(tenantId, isActive);
        return Ok(roles);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleDto dto)
    {
        if (id != dto.Id)
            return BadRequest("ID mismatch");

        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _roleService.UpdateAsync(dto, tenantId, userId);
        return Ok(result);
    }

    [HttpPost("{id}/permissions")]
    public async Task<IActionResult> AssignPermissions(Guid id, [FromBody] AssignPermissionsDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _roleService.AssignPermissionsAsync(id, dto, tenantId, userId);
        return Ok(result);
    }

    [HttpGet("{id}/permissions")]
    public async Task<IActionResult> GetPermissions(Guid id)
    {
        var tenantId = GetTenantId();
        var permissions = await _roleService.GetPermissionsAsync(id, tenantId);
        return Ok(permissions);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _roleService.DeleteAsync(id, tenantId);
        if (!result)
            return NotFound();
        return NoContent();
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();

    private Guid GetUserId() => User.RequireUserId();
}