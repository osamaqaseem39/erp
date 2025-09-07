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
        try
        {
            var result = await _paymentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result }, new { PaymentId = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var payment = await _paymentService.GetByIdAsync(id);
            if (payment == null)
                return NotFound();
            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment {PaymentId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? customerId = null, [FromQuery] Guid? invoiceId = null, [FromQuery] string? paymentMethod = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all payments");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaymentDto dto)
    {
        try
        {
            dto.PaymentId = id;
            var result = await _paymentService.UpdateAsync(dto);
            return Ok(new { Success = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment {PaymentId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/refund")]
    public async Task<IActionResult> Refund(Guid id, [FromBody] RefundDto dto)
    {
        try
        {
            // Note: IPaymentService doesn't have RefundAsync method
            // This would need to be implemented or use a different approach
            return BadRequest(new { Error = "Refund functionality not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding payment {PaymentId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/void")]
    public async Task<IActionResult> Void(Guid id, [FromBody] VoidPaymentDto dto)
    {
        try
        {
            // Note: IPaymentService doesn't have VoidAsync method
            // This would need to be implemented or use a different approach
            return BadRequest(new { Error = "Void functionality not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voiding payment {PaymentId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _paymentService.DeleteAsync(id);
            if (!result)
                return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment {PaymentId}", id);
            return BadRequest(new { Error = ex.Message });
        }
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