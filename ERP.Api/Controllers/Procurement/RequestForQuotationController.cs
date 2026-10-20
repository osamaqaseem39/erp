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
[Route("api/procurement/rfqs")]
[Authorize]
[PermissionResource("procurement.rfqs")]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
public class RequestForQuotationController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IRequestForQuotationService _service;

    public RequestForQuotationController(
        IRequestForQuotationService service,
        ILogger<RequestForQuotationController> logger) : base(logger)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateRequestForQuotationDto dto)
    {
        var id = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return CreatedAtAction(nameof(GetById), new { id }, new { RfqId = id });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RequestForQuotationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return item == null ? NotFound() : Ok(item);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RequestForQuotationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null)
    {
        var tenantId = HttpContext.RequireTenantId();
        var items = string.IsNullOrEmpty(status)
            ? await _service.GetAllAsync(tenantId)
            : await _service.GetByStatusAsync(status, tenantId);
        return Ok(items);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRequestForQuotationDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.ValidationError, Message = "ID mismatch" });
        return await _service.UpdateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new MessageResponse { Message = "RFQ updated" })
            : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id) =>
        await _service.DeleteAsync(id, HttpContext.RequireTenantId()) ? NoContent() : NotFound();

    [HttpPost("{id:guid}/send")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Send(Guid id) =>
        await _service.SendAsync(id, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new MessageResponse { Message = "RFQ opened" })
            : BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Cannot send RFQ in current state." });

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Cancel(Guid id) =>
        await _service.CancelAsync(id, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new MessageResponse { Message = "Cancelled" })
            : BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Cannot cancel RFQ in current state." });

    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Close(Guid id) =>
        await _service.CloseAsync(id, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new MessageResponse { Message = "Closed" })
            : BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Cannot close RFQ in current state." });

    [HttpPost("{id:guid}/responses")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SubmitResponse(Guid id, [FromBody] CreateRFQVendorResponseDto dto)
    {
        dto.RFQId = id;
        var responseId = await _service.SubmitVendorResponseAsync(
            dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return Ok(new { ResponseId = responseId });
    }

    [HttpPost("{id:guid}/award/{responseId:guid}")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Award(Guid id, Guid responseId) =>
        await _service.AwardAsync(id, responseId, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new MessageResponse { Message = "Awarded" })
            : BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Cannot award RFQ in current state." });
}
