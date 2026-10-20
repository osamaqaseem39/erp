using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Application.Interfaces.HRM;
using ERP.Shared.DTOs.HRM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/leave-balances")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class LeaveBalanceController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ILeaveBalanceService _service;

    public LeaveBalanceController(ILeaveBalanceService service, ILogger<LeaveBalanceController> logger) : base(logger) =>
        _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LeaveBalanceDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? employeeId = null,
        [FromQuery] Guid? leaveTypeId = null) =>
        Ok(await _service.GetAllAsync(HttpContext.RequireTenantId(), employeeId, leaveTypeId));
}
