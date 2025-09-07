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
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _roleService.CreateAsync(dto, tenantId, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var role = await _roleService.GetByIdAsync(id, tenantId);
            if (role == null)
                return NotFound();
            return Ok(role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role {RoleId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null)
    {
        try
        {
            var tenantId = GetTenantId();
            var roles = await _roleService.GetAllAsync(tenantId, isActive);
            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all roles");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleDto dto)
    {
        try
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch");

            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _roleService.UpdateAsync(dto, tenantId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/permissions")]
    public async Task<IActionResult> AssignPermissions(Guid id, [FromBody] AssignPermissionsDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _roleService.AssignPermissionsAsync(id, dto, tenantId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning permissions to role {RoleId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("{id}/permissions")]
    public async Task<IActionResult> GetPermissions(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var permissions = await _roleService.GetPermissionsAsync(id, tenantId);
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for role {RoleId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _roleService.DeleteAsync(id, tenantId);
            if (!result)
                return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    private Guid GetTenantId()
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (Guid.TryParse(tenantIdClaim, out var tenantId))
            return tenantId;
        throw new UnauthorizedAccessException("Tenant ID not found in token");
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;
        throw new UnauthorizedAccessException("User ID not found in token");
    }
}