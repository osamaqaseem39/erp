using ERP.Api.Abstractions;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Interfaces.Procurement;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Procurement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Procurement;

[ApiController]
[Route("api/procurement/goods-receipt-notes")]
[Authorize]
[PermissionResource("procurement.goodsReceiptNotes")]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
public class GoodsReceiptNoteController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IGoodsReceiptNoteService _service;

    public GoodsReceiptNoteController(IGoodsReceiptNoteService service, ILogger<GoodsReceiptNoteController> logger) : base(logger)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateGoodsReceiptNoteDto dto)
    {
        var id = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return CreatedAtAction(nameof(GetById), new { id }, new { GoodsReceiptNoteId = id });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GoodsReceiptNoteDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return item == null ? NotFound() : Ok(item);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GoodsReceiptNoteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? purchaseOrderId = null, [FromQuery] string? status = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var tenantId = HttpContext.RequireTenantId();
        IEnumerable<GoodsReceiptNoteDto> items =
            purchaseOrderId.HasValue ? await _service.GetByPurchaseOrderAsync(purchaseOrderId.Value, tenantId)
            : !string.IsNullOrEmpty(status) ? await _service.GetByStatusAsync(status, tenantId)
            : fromDate.HasValue && toDate.HasValue ? await _service.GetByDateRangeAsync(fromDate.Value, toDate.Value, tenantId)
            : await _service.GetAllAsync(tenantId);
        return Ok(items);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGoodsReceiptNoteDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.ValidationError, Message = "ID mismatch" });
        return await _service.UpdateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new MessageResponse { Message = "GRN updated successfully" })
            : NotFound();
    }

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Approve(Guid id) =>
        await _service.ApproveAsync(id, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new MessageResponse { Message = "Posted" })
            : BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Cannot post GRN in current state." });

    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reject(Guid id) =>
        await _service.RejectAsync(id, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new MessageResponse { Message = "Cancelled" })
            : BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Cannot cancel GRN in current state." });

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id) =>
        await _service.DeleteAsync(id, HttpContext.RequireTenantId()) ? NoContent() : NotFound();
}
