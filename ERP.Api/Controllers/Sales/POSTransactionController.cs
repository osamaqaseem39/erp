using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Sales;
using ERP.Shared.DTOs.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Sales;

[ApiController]
[Route("api/sales/pos-transactions")]
[Authorize]
public class POSTransactionController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IPOSTransactionService _service;
    private readonly ILogger<POSTransactionController> _logger;

    public POSTransactionController(IPOSTransactionService service, ILogger<POSTransactionController> logger) : base(logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePOSTransactionDto dto)
    {
        try
        {
            var tenantId = GetTenantIdFromContext();
            var userId = GetUserIdFromContext();
            var result = await _service.CreateAsync(dto, tenantId, userId);
            return CreatedAtAction(nameof(GetById), new { id = result }, new { TransactionId = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating POS transaction");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var tenantId = GetTenantIdFromContext();
            var transaction = await _service.GetByIdAsync(id, tenantId);
            if (transaction == null)
                return NotFound();
            return Ok(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting POS transaction {TransactionId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? sessionId = null, [FromQuery] Guid? customerId = null, 
        [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var tenantId = GetTenantIdFromContext();
            IEnumerable<POSTransactionDto> transactions;

            if (sessionId.HasValue)
                transactions = await _service.GetBySessionAsync(sessionId.Value, tenantId);
            else if (customerId.HasValue)
                transactions = await _service.GetByCustomerAsync(customerId.Value, tenantId);
            else if (startDate.HasValue && endDate.HasValue)
                transactions = await _service.GetByDateRangeAsync(startDate.Value, endDate.Value, tenantId);
            else
                transactions = await _service.GetAllAsync(tenantId);

            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting POS transactions");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePOSTransactionDto dto)
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
            return Ok(new { Message = "POS transaction updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating POS transaction {TransactionId}", id);
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
            return Ok(new { Message = "POS transaction deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting POS transaction {TransactionId}", id);
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

