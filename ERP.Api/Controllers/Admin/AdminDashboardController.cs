using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Admin;
using ERP.Shared.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize]
[PermissionResource("admin.dashboard")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _dashboard;

    public AdminDashboardController(IAdminDashboardService dashboard) => _dashboard = dashboard;

    /// <summary>Admin home: user/role counts, sign-ins, recent permission changes (all real queries).</summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(AdminDashboardDto), OpenApiResponseTypes.Ok)]
    public async Task<ActionResult<AdminDashboardDto>> Summary(CancellationToken cancellationToken)
        => Ok(await _dashboard.GetSummaryAsync(HttpContext.RequireTenantId(), cancellationToken));
}
