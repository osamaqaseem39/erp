using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/bills")]
[Authorize]
public class BillsController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IBillService _billService;
    private readonly ILogger<BillsController> _logger;

    public BillsController(IBillService billService, ILogger<BillsController> logger) : base(logger)
    {
        _billService = billService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBillDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _billService.CreateAsync(dto, tenantId, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bill");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var bill = await _billService.GetByIdAsync(id, tenantId);
            if (bill == null)
                return NotFound();
            return Ok(bill);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bill {BillId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? vendorId = null, [FromQuery] string? status = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var tenantId = GetTenantId();
            var bills = await _billService.GetAllAsync(tenantId, vendorId, status, fromDate, toDate);
            return Ok(bills);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all bills");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBillDto dto)
    {
        try
        {
            if (id != dto.BillId)
                return BadRequest("ID mismatch");

            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _billService.UpdateAsync(dto, tenantId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating bill {BillId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _billService.ApproveAsync(id, tenantId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving bill {BillId}", id);
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
            var result = await _billService.MarkAsPaidAsync(id, paymentDto, tenantId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking bill as paid {BillId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/void")]
    public async Task<IActionResult> VoidBill(Guid id, [FromBody] VoidBillDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _billService.VoidBillAsync(id, dto, tenantId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voiding bill {BillId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _billService.DeleteAsync(id, tenantId);
            if (!result)
                return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting bill {BillId}", id);
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