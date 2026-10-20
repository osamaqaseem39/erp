using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Interfaces.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Manufacturing;

[ApiController]
[Route("api/manufacturing/work-centers")]
[Authorize]
[PermissionResource("manufacturing.workCenters")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class WorkCenterCapacityController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IProductionOrderService _orders;

    public WorkCenterCapacityController(IProductionOrderService orders, ILogger<WorkCenterCapacityController> logger)
        : base(logger) => _orders = orders;

    [HttpGet("{id:guid}/capacity-warn")]
    [ProducesResponseType(typeof(WorkCenterCapacityWarnDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> CapacityWarn(Guid id, [FromQuery] DateTime? weekStartUtc = null) =>
        Ok(await _orders.GetCapacityWarnAsync(id, HttpContext.RequireTenantId(), weekStartUtc));
}
