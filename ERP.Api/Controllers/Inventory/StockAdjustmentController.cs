using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Inventory;
using ERP.Shared.DTOs.Inventory;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/adjustments")]
[Authorize]
public class StockAdjustmentController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IStockAdjustmentService _adjustmentService;
    private readonly ILogger<StockAdjustmentController> _logger;

    public StockAdjustmentController(IStockAdjustmentService adjustmentService, ILogger<StockAdjustmentController> logger) : base(logger)
    {
        _adjustmentService = adjustmentService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStockAdjustmentDto dto)
    {
        try
        {
            var result = await _adjustmentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result }, new { AdjustmentId = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating stock adjustment");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var adjustment = await _adjustmentService.GetByIdAsync(id);
            if (adjustment == null)
                return NotFound();
            return Ok(adjustment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock adjustment {AdjustmentId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? warehouseId = null, [FromQuery] string? reason = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var adjustments = await _adjustmentService.GetAllAsync();
            return Ok(adjustments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all stock adjustments");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStockAdjustmentDto dto)
    {
        try
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch");

            var result = await _adjustmentService.UpdateAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock adjustment {AdjustmentId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        try
        {
            // Note: ApproveAsync not available in current interface
            return BadRequest(new { Error = "Approve functionality not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving stock adjustment {AdjustmentId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectAdjustmentDto dto)
    {
        try
        {
            // Note: RejectAsync not available in current interface
            return BadRequest(new { Error = "Reject functionality not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting stock adjustment {AdjustmentId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _adjustmentService.DeleteAsync(id);
            if (!result)
                return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting stock adjustment {AdjustmentId}", id);
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