using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/invoices")]
[Authorize]
public class InvoicesController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly ILogger<InvoicesController> _logger;

    public InvoicesController(IInvoiceService invoiceService, ILogger<InvoicesController> logger) : base(logger)
    {
        _invoiceService = invoiceService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] InvoiceCreateDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _invoiceService.AddInvoiceAsync(dto, tenantId, userId);
            return CreatedAtAction(nameof(GetById), new { id = result }, new { InvoiceId = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id, tenantId);
            if (invoice == null)
                return NotFound();
            return Ok(invoice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invoice {InvoiceId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? customerId = null, [FromQuery] string? status = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        try
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
            
            var (invoices, totalCount) = await _invoiceService.SearchInvoicesAsync(query);
            return Ok(invoices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all invoices");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] InvoiceUpdateDto dto)
    {
        try
        {
            dto.InvoiceId = id;
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _invoiceService.UpdateInvoiceAsync(dto, tenantId, userId);
            return Ok(new { Success = result != Guid.Empty });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating invoice {InvoiceId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/send")]
    public async Task<IActionResult> SendInvoice(Guid id)
    {
        try
        {
            // Note: IInvoiceService doesn't have SendInvoiceAsync method
            // This would need to be implemented or use a different approach
            return BadRequest(new { Error = "Send invoice functionality not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending invoice {InvoiceId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/mark-paid")]
    public async Task<IActionResult> MarkAsPaid(Guid id, [FromBody] PaymentDto paymentDto)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking invoice as paid {InvoiceId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/void")]
    public async Task<IActionResult> VoidInvoice(Guid id, [FromBody] VoidInvoiceDto dto)
    {
        try
        {
            // Note: IInvoiceService doesn't have VoidInvoiceAsync method
            // This would need to be implemented or use a different approach
            return BadRequest(new { Error = "Void functionality not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voiding invoice {InvoiceId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _invoiceService.DeleteInvoiceAsync(id, tenantId);
            if (!result)
                return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting invoice {InvoiceId}", id);
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