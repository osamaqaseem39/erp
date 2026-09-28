using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Procurement;
using ERP.Shared.DTOs.Procurement;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Procurement;

[ApiController]
[Route("api/procurement/purchase-orders")]
[Authorize]
public class PurchaseOrderController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IPurchaseOrderService _purchaseOrderService;
    private readonly ILogger<PurchaseOrderController> _logger;

    public PurchaseOrderController(IPurchaseOrderService purchaseOrderService, ILogger<PurchaseOrderController> logger) : base(logger)
    {
        _purchaseOrderService = purchaseOrderService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _purchaseOrderService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result }, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = GetTenantId();
        var order = await _purchaseOrderService.GetByIdAsync(id);
        if (order == null)
            return NotFound();
        return Ok(order);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? vendorId = null, [FromQuery] string? status = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        IEnumerable<PurchaseOrderDto> orders;
        
        if (vendorId.HasValue)
            orders = await _purchaseOrderService.GetByVendorAsync(vendorId.Value);
        else if (!string.IsNullOrEmpty(status))
            orders = await _purchaseOrderService.GetByStatusAsync(status);
        else if (fromDate.HasValue && toDate.HasValue)
            orders = await _purchaseOrderService.GetByDateRangeAsync(fromDate.Value, toDate.Value);
        else
            orders = await _purchaseOrderService.GetAllAsync();
            
        return Ok(orders);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePurchaseOrderDto dto)
    {
        if (id != dto.Id)
            return BadRequest("ID mismatch");

        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _purchaseOrderService.UpdateAsync(dto);
        return Ok(result);
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _purchaseOrderService.ApproveAsync(id);
        return Ok(result);
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        var result = await _purchaseOrderService.RejectAsync(id);
        return Ok(result);
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _purchaseOrderService.DeleteAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
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