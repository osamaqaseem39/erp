using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Manufacturing;

[ApiController]
[Route("api/manufacturing/production-scrap")]
[Authorize]
[PermissionResource("manufacturing.productionScrap")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class ProductionScrapController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IProductionScrapService _service;

    public ProductionScrapController(IProductionScrapService service, ILogger<ProductionScrapController> logger) : base(logger) =>
        _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductionScrapDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> List([FromQuery] Guid? productionOrderId = null) =>
        Ok(await _service.ListAsync(HttpContext.RequireTenantId(), productionOrderId));

    [HttpPost]
    [ProducesResponseType(typeof(ProductionScrapDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateProductionScrapDto dto) =>
        Ok(await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId()));
}
