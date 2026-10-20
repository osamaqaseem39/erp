using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Api.Abstractions;
using ERP.Api.OpenApi;
using ERP.Application.Interfaces.Sales;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Sales;

[ApiController]
[Route("api/sales/credit-notes")]
[Authorize]
[PermissionResource("sales.creditNotes")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class SalesCreditNoteController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ISalesCreditNoteService _service;

    public SalesCreditNoteController(ISalesCreditNoteService service, ILogger<SalesCreditNoteController> logger)
        : base(logger)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateSalesCreditNoteDto dto)
    {
        var id = await _service.CreateAsync(dto, GetTenantIdFromContext(), GetUserIdFromContext());
        return CreatedAtAction(nameof(GetById), new { id }, new { CreditNoteId = id });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SalesCreditNoteDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var note = await _service.GetByIdAsync(id, GetTenantIdFromContext());
        if (note == null) return NotFound();
        return Ok(note);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SalesCreditNoteDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? customerId = null, [FromQuery] string? status = null)
    {
        var tenantId = GetTenantIdFromContext();
        IEnumerable<SalesCreditNoteDto> notes;
        if (customerId.HasValue)
            notes = await _service.GetByCustomerAsync(customerId.Value, tenantId);
        else if (!string.IsNullOrEmpty(status))
            notes = await _service.GetByStatusAsync(status, tenantId);
        else
            notes = await _service.GetAllAsync(tenantId);
        return Ok(notes);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSalesCreditNoteDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.ValidationError, Message = "ID mismatch" });
        var ok = await _service.UpdateAsync(dto, GetTenantIdFromContext(), GetUserIdFromContext());
        if (!ok) return NotFound();
        return Ok(new MessageResponse { Message = "Credit note updated successfully" });
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteAsync(id, GetTenantIdFromContext());
        if (!ok) return NotFound();
        return Ok(new MessageResponse { Message = "Credit note deleted successfully" });
    }

    [HttpPost("{id:guid}/post")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Post(Guid id)
    {
        var ok = await _service.PostAsync(id, GetTenantIdFromContext(), GetUserIdFromContext());
        if (!ok)
            return BadRequest(new ApiError
            {
                Status = 400,
                ErrorCode = ApiErrorCodes.BusinessRule,
                Message = "Unable to post credit note (check SI posted, AR balance, status)"
            });
        return Ok(new MessageResponse { Message = "Credit note posted successfully" });
    }

    [HttpPost("{id:guid}/void")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Void(Guid id, [FromBody] VoidSalesCreditNoteDto? dto = null)
    {
        var ok = await _service.VoidAsync(id, dto ?? new VoidSalesCreditNoteDto(), GetTenantIdFromContext(), GetUserIdFromContext());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to void credit note" });
        return Ok(new MessageResponse { Message = "Credit note voided successfully" });
    }

    private Guid GetTenantIdFromContext() => HttpContext.RequireTenantId();
    private Guid GetUserIdFromContext() => User.RequireUserId();
}
