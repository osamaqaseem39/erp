using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/payments")]
[Authorize]
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
    public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
    {
        var result = await _paymentService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result }, new { PaymentId = result });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var payment = await _paymentService.GetByIdAsync(id);
        if (payment == null)
            return NotFound();
        return Ok(payment);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? customerId = null, [FromQuery] Guid? invoiceId = null, [FromQuery] string? paymentMethod = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        IEnumerable<PaymentDto> payments;
        
        if (invoiceId.HasValue)
            payments = await _paymentService.GetByInvoiceAsync(invoiceId.Value);
        else if (fromDate.HasValue && toDate.HasValue)
            payments = await _paymentService.GetByDateRangeAsync(fromDate.Value, toDate.Value);
        else
            payments = await _paymentService.GetAllAsync();
            
        return Ok(payments);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaymentDto dto)
    {
        dto.PaymentId = id;
        var result = await _paymentService.UpdateAsync(dto);
        return Ok(new { Success = result });
    }

    [HttpPost("{id}/refund")]
    public async Task<IActionResult> Refund(Guid id, [FromBody] RefundDto dto)
    {
        // Note: IPaymentService doesn't have RefundAsync method
        // This would need to be implemented or use a different approach
        return BadRequest(new { Error = "Refund functionality not yet implemented" });
    }

    [HttpPost("{id}/void")]
    public async Task<IActionResult> Void(Guid id, [FromBody] VoidPaymentDto dto)
    {
        // Note: IPaymentService doesn't have VoidAsync method
        // This would need to be implemented or use a different approach
        return BadRequest(new { Error = "Void functionality not yet implemented" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _paymentService.DeleteAsync(id);
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