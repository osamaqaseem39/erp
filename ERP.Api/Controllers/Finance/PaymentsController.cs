using ERP.Api.Security;
using ERP.Api.OpenApi;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/payments")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class PaymentsController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger) : base(logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
    {
        dto.TenantId = GetTenantId();
        dto.CreatedBy = GetUserId().ToString();
        var result = await _paymentService.CreateAsync(dto);
        if (result == Guid.Empty)
            return BadRequest(new { Error = "Could not create payment (tenant required)." });
        return CreatedAtAction(nameof(GetById), new { id = result }, new { PaymentId = result });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PaymentDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var payment = await _paymentService.GetByIdAsync(id, GetTenantId());
        if (payment == null)
            return NotFound();
        return Ok(payment);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PaymentDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? invoiceId = null,
        [FromQuery] string? paymentMethod = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var tenantId = GetTenantId();
        IEnumerable<PaymentDto> payments;

        if (invoiceId.HasValue)
            payments = await _paymentService.GetByInvoiceAsync(invoiceId.Value, tenantId);
        else if (fromDate.HasValue && toDate.HasValue)
            payments = await _paymentService.GetByDateRangeAsync(fromDate.Value, toDate.Value, tenantId);
        else
            payments = await _paymentService.GetAllAsync(tenantId);

        return Ok(payments);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaymentDto dto)
    {
        dto.PaymentId = id;
        var result = await _paymentService.UpdateAsync(dto, GetTenantId(), GetUserId());
        return Ok(new { Success = result });
    }

    [HttpPost("{id}/refund")]
    [ProducesResponseType(typeof(PaymentDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Refund(Guid id, [FromBody] RefundDto dto)
    {
        dto ??= new RefundDto();
        dto.PaymentId = id;
        var tenantId = GetTenantId();
        var ok = await _paymentService.RefundAsync(id, dto, tenantId, GetUserId());
        if (!ok)
            return BadRequest(new { Error = "Could not refund payment (missing, voided, or unapply failed)." });
        return Ok(await _paymentService.GetByIdAsync(id, tenantId));
    }

    [HttpPost("{id}/void")]
    [ProducesResponseType(typeof(PaymentDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Void(Guid id, [FromBody] VoidPaymentDto dto)
    {
        dto ??= new VoidPaymentDto();
        dto.PaymentId = id;
        var tenantId = GetTenantId();
        var ok = await _paymentService.VoidAsync(id, dto, tenantId, GetUserId());
        if (!ok) return BadRequest(new { Error = "Could not void payment (missing, already voided, or unapply failed)." });
        return Ok(await _paymentService.GetByIdAsync(id, tenantId));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _paymentService.DeleteAsync(id, GetTenantId());
        if (!result)
            return NotFound();
        return NoContent();
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();

    private Guid GetUserId() => User.RequireUserId();
}
