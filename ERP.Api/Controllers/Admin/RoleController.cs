using ERP.Api.Security.Permissions;
using ERP.Api.Security;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Admin;
using ERP.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using PermissionDto = ERP.Shared.DTOs.Core.PermissionDto;
using RoleDto = ERP.Shared.DTOs.Admin.RoleDto;
using CreateRoleDto = ERP.Shared.DTOs.Admin.CreateRoleDto;
using UpdateRoleDto = ERP.Shared.DTOs.Admin.UpdateRoleDto;
using AssignPermissionsDto = ERP.Shared.DTOs.Admin.AssignPermissionsDto;

namespace ERP.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/roles")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class RoleController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService, ILogger<RoleController> logger) : base(logger)
    {
        _roleService = roleService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(RoleDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
    {
        var result = await _roleService.CreateAsync(dto, GetTenantId(), GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RoleDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var role = await _roleService.GetByIdAsync(id, GetTenantId());
        return role == null ? NotFound() : Ok(role);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoleDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null) =>
        Ok(await _roleService.GetAllAsync(GetTenantId(), isActive));

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RoleDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.ValidationError, Message = "ID mismatch" });

        var result = await _roleService.UpdateAsync(dto, GetTenantId(), GetUserId());
        return Ok(result);
    }

    [HttpPost("{id}/permissions")]
    [RequirePermission("admin.roles.assign")]
    [ProducesResponseType(typeof(bool), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> AssignPermissions(Guid id, [FromBody] AssignPermissionsDto dto) =>
        Ok(await _roleService.AssignPermissionsAsync(id, dto, GetTenantId(), GetUserId()));

    [HttpGet("{id}/permissions")]
    [ProducesResponseType(typeof(IEnumerable<PermissionDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetPermissions(Guid id) =>
        Ok(await _roleService.GetPermissionsAsync(id, GetTenantId()));

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _roleService.DeleteAsync(id, GetTenantId());
        return result ? NoContent() : NotFound();
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
