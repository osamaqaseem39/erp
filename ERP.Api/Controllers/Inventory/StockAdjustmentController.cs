using ERP.Api.Security;
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
        var result = await _adjustmentService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result }, new { AdjustmentId = result });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var adjustment = await _adjustmentService.GetByIdAsync(id);
        if (adjustment == null)
            return NotFound();
        return Ok(adjustment);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? warehouseId = null, [FromQuery] string? reason = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var adjustments = await _adjustmentService.GetAllAsync();
        return Ok(adjustments);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStockAdjustmentDto dto)
    {
        if (id != dto.Id)
            return BadRequest("ID mismatch");

        var result = await _adjustmentService.UpdateAsync(dto);
        return Ok(result);
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        // Note: ApproveAsync not available in current interface
        return BadRequest(new { Error = "Approve functionality not yet implemented" });
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectAdjustmentDto dto)
    {
        // Note: RejectAsync not available in current interface
        return BadRequest(new { Error = "Reject functionality not yet implemented" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _adjustmentService.DeleteAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();

    private Guid GetUserId() => User.RequireUserId();
}