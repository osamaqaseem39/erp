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
[Route("api/procurement/purchase-returns")]
[Authorize]
[PermissionResource("procurement.purchaseReturns")]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
public class PurchaseReturnController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IPurchaseReturnService _service;

    public PurchaseReturnController(IPurchaseReturnService service, ILogger<PurchaseReturnController> logger)
        : base(logger)
    {
        _service = service;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PurchaseReturnDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return item == null ? NotFound() : Ok(item);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PurchaseReturnDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(HttpContext.RequireTenantId()));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseReturnDto dto)
    {
        var id = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        if (id == Guid.Empty)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to create purchase return." });
        return CreatedAtAction(nameof(GetById), new { id }, new { PurchaseReturnId = id });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePurchaseReturnDto dto)
    {
        dto.Id = id;
        return await _service.UpdateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new MessageResponse { Message = "Purchase return updated" })
            : BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to update (must be Pending)." });
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id) =>
        await _service.DeleteAsync(id, HttpContext.RequireTenantId()) ? NoContent() : NotFound();

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var ok = await _service.ApproveAsync(id, HttpContext.RequireTenantId(), User.RequireUserId());
        return ok
            ? Ok(new MessageResponse { Message = "Approved" })
            : BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to approve return." });
    }

    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(PurchaseReturnDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Complete(Guid id)
    {
        var tenantId = HttpContext.RequireTenantId();
        var ok = await _service.CompleteAsync(id, tenantId, User.RequireUserId());
        if (!ok)
            return BadRequest(new ApiError
            {
                Status = 400,
                ErrorCode = ApiErrorCodes.BusinessRule,
                Message = "Unable to complete return (must be Approved; stock/debit failed)."
            });
        return Ok(await _service.GetByIdAsync(id, tenantId));
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var ok = await _service.CancelAsync(id, HttpContext.RequireTenantId(), User.RequireUserId());
        return ok
            ? Ok(new MessageResponse { Message = "Cancelled" })
            : BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to cancel return." });
    }
}
