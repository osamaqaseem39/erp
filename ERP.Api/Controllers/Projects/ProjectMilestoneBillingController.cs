using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Projects;
using ERP.Shared.DTOs.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Projects;

[ApiController]
[Route("api/projects/milestones")]
[Authorize]
[PermissionResource("projects.milestones")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class ProjectMilestoneBillingController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IProjectMilestoneBillingService _billing;

    public ProjectMilestoneBillingController(
        IProjectMilestoneBillingService billing,
        ILogger<ProjectMilestoneBillingController> logger) : base(logger) =>
        _billing = billing;

    [HttpPost("{milestoneId:guid}/bill")]
    [ProducesResponseType(typeof(MilestoneBillingResultDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Bill(Guid milestoneId, [FromBody] MilestoneBillingDto dto) =>
        Ok(await _billing.CreateDraftInvoiceFromMilestoneAsync(milestoneId, dto, HttpContext.RequireTenantId(), User.RequireUserId()));
}
