using ERP.Api.Security;
using ERP.Api.Abstractions;
using ERP.Api.Security.Permissions;
using ERP.Application.Interfaces.Core;
using ERP.Infrastructure.Security;
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

    /// <summary>Every permission the application enforces, grouped by module, for the role editor.</summary>
    [HttpGet("catalog")]
    public IActionResult GetCatalog([FromServices] IPermissionCatalog catalog)
    {
        var modules = catalog.Permissions
            .GroupBy(p => p.Module)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                module = g.Key,
                name = EndpointPermissions.ModuleName(g.Key),
                resources = g.GroupBy(p => p.Resource).OrderBy(r => r.Key).Select(r => new
                {
                    resource = r.Key,
                    permissions = r.Select(p => new { code = p.Code, action = p.Action, description = p.Description }),
                }),
            });
        return Ok(modules);
    }

    /// <summary>Re-runs permission and built-in role seeding for the caller's organization (also runs at startup).</summary>
    [HttpPost("seed")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> SeedDefaults([FromServices] IRbacSeeder seeder)
    {
        var result = await seeder.SeedTenantAsync(GetTenantId());
        return Ok(result);
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
}
