using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Manufacturing;

[ApiController]
[Route("api/manufacturing/production-runs")]
[Authorize]
[PermissionResource("manufacturing.productionOrders")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class ProductionRunController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IProductionRunService _service;

    public ProductionRunController(IProductionRunService service, ILogger<ProductionRunController> logger) : base(logger)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductionRunDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id, GetTenantId());
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("by-order/{productionOrderId}")]
    [ProducesResponseType(typeof(IEnumerable<ProductionRunDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByOrder(Guid productionOrderId)
    {
        var result = await _service.GetByOrderAsync(productionOrderId, GetTenantId());
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateProductionRunDto dto)
    {
        var result = await _service.CreateAsync(dto, GetTenantId(), GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(ProductionRunDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteProductionRunDto dto)
    {
        var result = await _service.CompleteAsync(
            id, dto.QuantityProduced, dto.QuantityRejected, GetTenantId(), GetUserId());
        return Ok(result);
    }

    [HttpPost("{id}/cancel")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var ok = await _service.CancelAsync(id, GetTenantId(), GetUserId());
        return ok ? Ok() : NotFound();
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}

public class CompleteProductionRunDto
{
    public decimal QuantityProduced { get; set; }
    public decimal QuantityRejected { get; set; }
}
