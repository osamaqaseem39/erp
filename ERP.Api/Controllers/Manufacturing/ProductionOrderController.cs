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
[Route("api/manufacturing/production-orders")]
[Authorize]
[PermissionResource("manufacturing.productionOrders")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class ProductionOrderController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IProductionOrderService _productionOrderService;
    private readonly ILogger<ProductionOrderController> _logger;

    public ProductionOrderController(IProductionOrderService productionOrderService, ILogger<ProductionOrderController> logger) : base(logger)
    {
        _productionOrderService = productionOrderService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateProductionOrderDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _productionOrderService.CreateAsync(dto, tenantId, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductionOrderDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = GetTenantId();
        var order = await _productionOrderService.GetByIdAsync(id, tenantId);
        if (order == null)
            return NotFound();
        return Ok(order);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductionOrderDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null, [FromQuery] Guid? workCenterId = null)
    {
        var tenantId = GetTenantId();
        var orders = await _productionOrderService.GetAllAsync(tenantId, status, workCenterId);
        return Ok(orders);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductionOrderDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductionOrderDto dto)
    {
        if (id != dto.Id)
            return BadRequest("ID mismatch");

        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _productionOrderService.UpdateAsync(dto, tenantId, userId);
        return Ok(result);
    }

    [HttpPost("{id}/start")]
    [ProducesResponseType(typeof(ProductionOrderDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Start(Guid id)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _productionOrderService.StartAsync(id, tenantId, userId);
        return Ok(result);
    }

    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(ProductionOrderDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteProductionOrderDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        dto.ProductionOrderId = id;
        var result = await _productionOrderService.CompleteAsync(id, dto, tenantId, userId);
        return Ok(result);
    }

    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(ProductionOrderDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelProductionOrderDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _productionOrderService.CancelAsync(id, dto.Reason, tenantId, userId);
        return Ok(result);
    }

    [HttpGet("{id}/operations")]
    [ProducesResponseType(typeof(IEnumerable<ProductionOrderOperationDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ListOperations(Guid id)
    {
        var ops = await _productionOrderService.ListOperationsAsync(id, GetTenantId());
        return Ok(ops);
    }

    [HttpPost("{id}/operations/{operationId:guid}/complete")]
    [ProducesResponseType(typeof(ProductionOrderOperationDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> CompleteOperation(
        Guid id, Guid operationId, [FromBody] CompleteProductionOrderOperationDto? dto)
    {
        var result = await _productionOrderService.CompleteOperationAsync(
            id, operationId, dto ?? new CompleteProductionOrderOperationDto(), GetTenantId(), GetUserId());
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _productionOrderService.DeleteAsync(id, tenantId);
        if (!result)
            return NotFound();
        return NoContent();
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();

    private Guid GetUserId() => User.RequireUserId();
}