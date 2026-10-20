using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Procurement;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Procurement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Procurement;

[ApiController]
[Route("api/procurement/purchase-orders")]
[Authorize]
[PermissionResource("procurement.purchaseOrders")]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
public class PurchaseOrderController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IPurchaseOrderService _purchaseOrderService;

    public PurchaseOrderController(IPurchaseOrderService purchaseOrderService, ILogger<PurchaseOrderController> logger) : base(logger)
    {
        _purchaseOrderService = purchaseOrderService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _purchaseOrderService.CreateAsync(dto, tenantId, userId);
        return CreatedAtAction(nameof(GetById), new { id = result }, new { PurchaseOrderId = result });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PurchaseOrderDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = GetTenantId();
        var order = await _purchaseOrderService.GetByIdAsync(id, tenantId);
        if (order == null)
            return NotFound();
        return Ok(order);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PurchaseOrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? vendorId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var tenantId = GetTenantId();
        IEnumerable<PurchaseOrderDto> orders;

        if (vendorId.HasValue)
            orders = await _purchaseOrderService.GetByVendorAsync(vendorId.Value, tenantId);
        else if (!string.IsNullOrEmpty(status))
            orders = await _purchaseOrderService.GetByStatusAsync(status, tenantId);
        else if (fromDate.HasValue && toDate.HasValue)
            orders = await _purchaseOrderService.GetByDateRangeAsync(fromDate.Value, toDate.Value, tenantId);
        else
            orders = await _purchaseOrderService.GetAllAsync(tenantId);

        return Ok(orders);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePurchaseOrderDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.ValidationError, Message = "ID mismatch" });

        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _purchaseOrderService.UpdateAsync(dto, tenantId, userId);
        if (!result)
            return NotFound();
        return Ok(new MessageResponse { Message = "Purchase order updated successfully" });
    }

    [HttpPost("{id}/approve")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _purchaseOrderService.ApproveAsync(id, tenantId, userId);
        if (!result)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Purchase order cannot be approved in its current state." });
        return Ok(new MessageResponse { Message = "Purchase order approved successfully" });
    }

    [HttpPost("{id}/reject")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reject(Guid id)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _purchaseOrderService.RejectAsync(id, tenantId, userId);
        if (!result)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Purchase order cannot be rejected in its current state." });
        return Ok(new MessageResponse { Message = "Purchase order rejected successfully" });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _purchaseOrderService.DeleteAsync(id, tenantId);
        if (!result)
            return NotFound();
        return NoContent();
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();

    private Guid GetUserId() => User.RequireUserId();
}
