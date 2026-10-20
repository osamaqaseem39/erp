using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Inventory;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Inventory;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/adjustments")]
[Authorize]
[PermissionResource("inventory.adjustments")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class StockAdjustmentController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IStockAdjustmentService _adjustmentService;

    public StockAdjustmentController(IStockAdjustmentService adjustmentService, ILogger<StockAdjustmentController> logger)
        : base(logger)
    {
        _adjustmentService = adjustmentService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(IdResponse), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateStockAdjustmentDto dto)
    {
        var id = await _adjustmentService.CreateAsync(dto, GetTenantId(), GetUserId());
        if (id == Guid.Empty) return BadRequest(new { error = "Invalid adjustment." });
        return CreatedAtAction(nameof(GetById), new { id }, new IdResponse { Id = id });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(StockAdjustmentDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var adjustment = await _adjustmentService.GetByIdAsync(id, GetTenantId());
        return adjustment == null ? NotFound() : Ok(adjustment);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<StockAdjustmentDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] Guid? itemId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var tenantId = GetTenantId();
        IEnumerable<StockAdjustmentDto> adjustments =
            warehouseId.HasValue ? await _adjustmentService.GetByWarehouseAsync(warehouseId.Value, tenantId)
            : itemId.HasValue ? await _adjustmentService.GetByItemAsync(itemId.Value, tenantId)
            : fromDate.HasValue && toDate.HasValue
                ? await _adjustmentService.GetByDateRangeAsync(fromDate.Value, toDate.Value, tenantId)
                : await _adjustmentService.GetAllAsync(tenantId);
        return Ok(adjustments);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SuccessResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStockAdjustmentDto dto)
    {
        dto.Id = id;
        var ok = await _adjustmentService.UpdateAsync(dto, GetTenantId(), GetUserId());
        return ok ? Ok(new SuccessResponse { Success = true }) : BadRequest(new { error = "Update rejected (not Draft or invalid)." });
    }

    [HttpPost("{id}/approve")]
    [HttpPost("{id}/post")]
    [ProducesResponseType(typeof(SuccessResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Post(Guid id)
    {
        var ok = await _adjustmentService.PostAsync(id, GetTenantId(), GetUserId());
        return ok ? Ok(new SuccessResponse { Success = true }) : BadRequest(new { error = "Post rejected." });
    }

    [HttpPost("{id}/reject")]
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(SuccessResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] RejectAdjustmentDto? dto = null)
    {
        var ok = await _adjustmentService.CancelAsync(id, GetTenantId(), GetUserId());
        return ok ? Ok(new SuccessResponse { Success = true }) : BadRequest(new { error = "Cancel rejected." });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _adjustmentService.DeleteAsync(id, GetTenantId());
        return ok ? NoContent() : NotFound();
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
