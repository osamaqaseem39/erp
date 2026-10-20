using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/budget-enforcement")]
[Authorize]
[PermissionResource("finance.budgets")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
public class BudgetEnforcementController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IBudgetEnforcementService _service;

    public BudgetEnforcementController(IBudgetEnforcementService service, ILogger<BudgetEnforcementController> logger) : base(logger) =>
        _service = service;

    [HttpGet("journals/{journalId:guid}/would-exceed")]
    [ProducesResponseType(typeof(object), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> WouldExceed(Guid journalId)
    {
        var tenantId = HttpContext.RequireTenantId();
        if (tenantId == Guid.Empty) return Forbid();
        var exceed = await _service.WouldExceedBudgetAsync(tenantId, journalId);
        return Ok(new { journalId, wouldExceedBudget = exceed });
    }
}
