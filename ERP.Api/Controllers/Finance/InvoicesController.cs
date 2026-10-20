using ERP.Api.Security;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/invoices")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class InvoicesController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService, ILogger<InvoicesController> logger) : base(logger)
    {
        _invoiceService = invoiceService;
    }

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] InvoiceCreateDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _invoiceService.AddInvoiceAsync(dto, tenantId, userId);
        return CreatedAtAction(nameof(GetById), new { id = result }, new { InvoiceId = result });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(InvoiceDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = GetTenantId();
        var invoice = await _invoiceService.GetInvoiceByIdAsync(id, tenantId);
        if (invoice == null)
            return NotFound();
        return Ok(invoice);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<InvoiceDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? customerId = null, [FromQuery] string? status = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var tenantId = GetTenantId();
        var query = new InvoiceQuery
        {
            TenantId = tenantId,
            CustomerId = customerId,
            Status = status,
            StartDate = fromDate,
            EndDate = toDate,
            PageNumber = 1,
            PageSize = 1000
        };

        var (invoices, _) = await _invoiceService.SearchInvoicesAsync(query);
        return Ok(invoices);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] InvoiceUpdateDto dto)
    {
        dto.InvoiceId = id;
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _invoiceService.UpdateInvoiceAsync(dto, tenantId, userId);
        return Ok(new { Success = result != Guid.Empty });
    }

    [HttpPost("{id}/send")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> SendInvoice(Guid id)
    {
        var ok = await _invoiceService.SendInvoiceAsync(id, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError
            {
                Status = 400,
                ErrorCode = ApiErrorCodes.BusinessRule,
                Message = "Could not send invoice (missing, cancelled/voided, or customer has no email)."
            });
        return Ok(new MessageResponse { Message = "Invoice send queued." });
    }

    [HttpPost("{id}/mark-paid")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> MarkAsPaid(Guid id, [FromBody] PaymentDto paymentDto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var paymentApplication = new PaymentApplicationDto
        {
            InvoiceId = id,
            PaymentId = paymentDto.Id,
            Amount = paymentDto.Amount ?? 0m
        };
        var result = await _invoiceService.ApplyPaymentToInvoiceAsync(paymentApplication, tenantId, userId);
        return Ok(new { Success = result });
    }

    [HttpPost("{id}/void")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> VoidInvoice(Guid id, [FromBody] VoidInvoiceDto dto)
    {
        dto ??= new VoidInvoiceDto();
        dto.InvoiceId = id;
        var ok = await _invoiceService.VoidInvoiceAsync(id, dto, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError
            {
                Status = 400,
                ErrorCode = ApiErrorCodes.BusinessRule,
                Message = "Could not void invoice (missing, already cancelled, or payments still applied)."
            });
        return Ok(new { Success = true });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _invoiceService.DeleteInvoiceAsync(id, tenantId);
        if (!result)
            return NotFound();
        return NoContent();
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();

    private Guid GetUserId() => User.RequireUserId();
}
