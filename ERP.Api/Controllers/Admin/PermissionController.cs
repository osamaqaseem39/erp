using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/permissions")]
[Authorize]
public class PermissionController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionController(IPermissionService permissionService, ILogger<PermissionController> logger)
        : base(logger)
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true)
    {
        var tenantId = GetTenantId();
        var permissions = await _permissionService.GetAllAsync(tenantId, activeOnly);
        return Ok(permissions);
    }

    [HttpPost("seed")]
    public async Task<IActionResult> SeedDefaults()
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var permissions = await _permissionService.SeedDefaultPermissionsAsync(tenantId, userId.ToString());
        return Ok(permissions);
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
