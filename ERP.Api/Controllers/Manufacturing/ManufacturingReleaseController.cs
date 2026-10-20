using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Manufacturing;

[ApiController]
[Route("api/manufacturing/release")]
[Authorize]
[PermissionResource("manufacturing.productionOrders")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class ManufacturingReleaseController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IManufacturingReleaseService _service;

    public ManufacturingReleaseController(IManufacturingReleaseService service, ILogger<ManufacturingReleaseController> logger) : base(logger) =>
        _service = service;

    [HttpGet("pending-sales-orders")]
    [ProducesResponseType(typeof(IEnumerable<PendingSalesOrderForProductionDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> PendingSalesOrders() =>
        Ok(await _service.GetPendingSalesOrdersAsync(HttpContext.RequireTenantId()));

    [HttpPost("sales-order-line")]
    [ProducesResponseType(typeof(ProductionOrderDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> ReleaseSalesOrderLine([FromBody] ReleaseSalesOrderLineDto dto)
    {
        var result = await _service.ReleaseSalesOrderLineAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return Ok(result);
    }
}
