using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Sales;
using ERP.Shared.DTOs.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Sales;

[ApiController]
[Route("api/sales/invoices")]
[Authorize]
public class SalesInvoiceController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ISalesInvoiceService _service;
    private readonly ILogger<SalesInvoiceController> _logger;

    public SalesInvoiceController(ISalesInvoiceService service, ILogger<SalesInvoiceController> logger) : base(logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSalesInvoiceDto dto)
    {
        try
        {
            var tenantId = GetTenantIdFromContext();
            var userId = GetUserIdFromContext();
            var result = await _service.CreateAsync(dto, tenantId, userId);
            return CreatedAtAction(nameof(GetById), new { id = result }, new { InvoiceId = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sales invoice");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var tenantId = GetTenantIdFromContext();
            var invoice = await _service.GetByIdAsync(id, tenantId);
            if (invoice == null)
                return NotFound();
            return Ok(invoice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sales invoice {InvoiceId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? customerId = null, [FromQuery] string? status = null)
    {
        try
        {
            var tenantId = GetTenantIdFromContext();
            IEnumerable<SalesInvoiceDto> invoices;

            if (customerId.HasValue)
                invoices = await _service.GetByCustomerAsync(customerId.Value, tenantId);
            else if (!string.IsNullOrEmpty(status))
                invoices = await _service.GetByStatusAsync(status, tenantId);
            else
                invoices = await _service.GetAllAsync(tenantId);

            return Ok(invoices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sales invoices");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSalesInvoiceDto dto)
    {
        try
        {
            if (id != dto.Id)
                return BadRequest(new { Error = "ID mismatch" });

            var tenantId = GetTenantIdFromContext();
            var userId = GetUserIdFromContext();
            var result = await _service.UpdateAsync(dto, tenantId, userId);
            if (!result)
                return NotFound();
            return Ok(new { Message = "Sales invoice updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sales invoice {InvoiceId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var tenantId = GetTenantIdFromContext();
            var result = await _service.DeleteAsync(id, tenantId);
            if (!result)
                return NotFound();
            return Ok(new { Message = "Sales invoice deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sales invoice {InvoiceId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/post")]
    public async Task<IActionResult> PostInvoice(Guid id)
    {
        try
        {
            var tenantId = GetTenantIdFromContext();
            var userId = GetUserIdFromContext();
            var result = await _service.PostInvoiceAsync(id, tenantId, userId);
            if (!result)
                return NotFound();
            return Ok(new { Message = "Sales invoice posted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting sales invoice {InvoiceId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/void")]
    public async Task<IActionResult> VoidInvoice(Guid id)
    {
        try
        {
            var tenantId = GetTenantIdFromContext();
            var userId = GetUserIdFromContext();
            var result = await _service.VoidInvoiceAsync(id, tenantId, userId);
            if (!result)
                return NotFound();
            return Ok(new { Message = "Sales invoice voided successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voiding sales invoice {InvoiceId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    private Guid GetTenantIdFromContext()
    {
        if (HttpContext.Items.TryGetValue("TenantId", out var tenantId) && tenantId is Guid id)
            return id;
        return Guid.Empty;
    }

    private Guid GetUserIdFromContext()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}

