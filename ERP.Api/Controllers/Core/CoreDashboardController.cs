using ERP.Api.Security;
using ERP.Application.Core;
using ERP.Shared.DTOs.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Core;

[ApiController]
[Route("api/core/dashboard")]
[Authorize]
public class CoreDashboardController : ControllerBase
{
    private readonly ICoreDashboardService _dashboard;

    public CoreDashboardController(ICoreDashboardService dashboard)
    {
        _dashboard = dashboard;
    }

    /// <summary>System health for the current tenant; the tenant overview is added for SuperAdmin.</summary>
    [HttpGet("summary")]
    public async Task<ActionResult<CoreDashboardDto>> Summary(CancellationToken cancellationToken)
    {
        Guid? companyId = Guid.TryParse(User.FindFirst(CurrentUser.CompanyIdClaim)?.Value, out var id) ? id : null;
        return Ok(await _dashboard.GetSummaryAsync(HttpContext.RequireTenantId(), companyId, User.IsSuperAdmin(), cancellationToken));
    }
}
