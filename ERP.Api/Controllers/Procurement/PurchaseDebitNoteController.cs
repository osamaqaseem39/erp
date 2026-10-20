using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Interfaces.Procurement;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Procurement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Procurement;

[ApiController]
[Route("api/procurement/debit-notes")]
[Authorize]
[PermissionResource("procurement.debitNotes")]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
public class PurchaseDebitNoteController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IPurchaseDebitNoteService _service;

    public PurchaseDebitNoteController(IPurchaseDebitNoteService service, ILogger<PurchaseDebitNoteController> logger)
        : base(logger)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseDebitNoteDto dto)
    {
        var id = await _service.CreateAsync(dto, GetTenantIdFromContext(), GetUserIdFromContext());
        return CreatedAtAction(nameof(GetById), new { id }, new { DebitNoteId = id });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PurchaseDebitNoteDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var note = await _service.GetByIdAsync(id, GetTenantIdFromContext());
        if (note == null) return NotFound();
        return Ok(note);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PurchaseDebitNoteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? vendorId = null, [FromQuery] string? status = null)
    {
        var tenantId = GetTenantIdFromContext();
        IEnumerable<PurchaseDebitNoteDto> notes;
        if (vendorId.HasValue)
            notes = await _service.GetByVendorAsync(vendorId.Value, tenantId);
        else if (!string.IsNullOrEmpty(status))
            notes = await _service.GetByStatusAsync(status, tenantId);
        else
            notes = await _service.GetAllAsync(tenantId);
        return Ok(notes);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePurchaseDebitNoteDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.ValidationError, Message = "ID mismatch" });
        var ok = await _service.UpdateAsync(dto, GetTenantIdFromContext(), GetUserIdFromContext());
        if (!ok) return NotFound();
        return Ok(new MessageResponse { Message = "Debit note updated successfully" });
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteAsync(id, GetTenantIdFromContext());
        if (!ok) return NotFound();
        return Ok(new MessageResponse { Message = "Debit note deleted successfully" });
    }

    [HttpPost("{id:guid}/post")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Post(Guid id)
    {
        var ok = await _service.PostAsync(id, GetTenantIdFromContext(), GetUserIdFromContext());
        if (!ok)
            return BadRequest(new ApiError
            {
                Status = 400,
                ErrorCode = ApiErrorCodes.BusinessRule,
                Message = "Unable to post debit note (check PI approved, AP balance, status)"
            });
        return Ok(new MessageResponse { Message = "Debit note posted successfully" });
    }

    [HttpPost("{id:guid}/void")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Void(Guid id, [FromBody] VoidPurchaseDebitNoteDto? dto = null)
    {
        var ok = await _service.VoidAsync(id, dto ?? new VoidPurchaseDebitNoteDto(), GetTenantIdFromContext(), GetUserIdFromContext());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to void debit note" });
        return Ok(new MessageResponse { Message = "Debit note voided successfully" });
    }

    private Guid GetTenantIdFromContext() => HttpContext.RequireTenantId();
    private Guid GetUserIdFromContext() => User.RequireUserId();
}
