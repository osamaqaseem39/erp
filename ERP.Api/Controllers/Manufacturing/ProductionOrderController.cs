using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Manufacturing;

[ApiController]
[Route("api/manufacturing/production-orders")]
[Authorize]
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
    public async Task<IActionResult> Create([FromBody] CreateProductionOrderDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _productionOrderService.CreateAsync(dto, tenantId, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating production order");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var order = await _productionOrderService.GetByIdAsync(id, tenantId);
            if (order == null)
                return NotFound();
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting production order {ProductionOrderId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null, [FromQuery] Guid? workCenterId = null)
    {
        try
        {
            var tenantId = GetTenantId();
            var orders = await _productionOrderService.GetAllAsync(tenantId, status, workCenterId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all production orders");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductionOrderDto dto)
    {
        try
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch");

            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _productionOrderService.UpdateAsync(dto, tenantId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating production order {ProductionOrderId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> Start(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _productionOrderService.StartAsync(id, tenantId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting production order {ProductionOrderId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteProductionOrderDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _productionOrderService.CompleteAsync(id, tenantId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing production order {ProductionOrderId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelProductionOrderDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _productionOrderService.CancelAsync(id, dto.Reason, tenantId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling production order {ProductionOrderId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _productionOrderService.DeleteAsync(id, tenantId);
            if (!result)
                return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting production order {ProductionOrderId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    private Guid GetTenantId()
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (Guid.TryParse(tenantIdClaim, out var tenantId))
            return tenantId;
        throw new UnauthorizedAccessException("Tenant ID not found in token");
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;
        throw new UnauthorizedAccessException("User ID not found in token");
    }
}