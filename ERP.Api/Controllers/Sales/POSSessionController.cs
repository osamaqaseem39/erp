using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Sales;
using ERP.Shared.DTOs.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Sales;

[ApiController]
[Route("api/sales/pos-sessions")]
[Authorize]
public class POSSessionController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IPOSSessionService _service;
    private readonly ILogger<POSSessionController> _logger;

    public POSSessionController(IPOSSessionService service, ILogger<POSSessionController> logger) : base(logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePOSSessionDto dto)
    {
        try
        {
            var tenantId = GetTenantIdFromContext();
            var userId = GetUserIdFromContext();
            var result = await _service.CreateAsync(dto, tenantId, userId);
            return CreatedAtAction(nameof(GetById), new { id = result }, new { SessionId = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating POS session");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var tenantId = GetTenantIdFromContext();
            var session = await _service.GetByIdAsync(id, tenantId);
            if (session == null)
                return NotFound();
            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting POS session {SessionId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? cashierId = null, [FromQuery] string? status = null)
    {
        try
        {
            var tenantId = GetTenantIdFromContext();
            IEnumerable<POSSessionDto> sessions;

            if (cashierId.HasValue)
                sessions = await _service.GetByCashierAsync(cashierId.Value, tenantId);
            else if (!string.IsNullOrEmpty(status))
                sessions = await _service.GetByStatusAsync(status, tenantId);
            else
                sessions = await _service.GetAllAsync(tenantId);

            return Ok(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting POS sessions");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePOSSessionDto dto)
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
            return Ok(new { Message = "POS session updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating POS session {SessionId}", id);
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
            return Ok(new { Message = "POS session deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting POS session {SessionId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/close")]
    public async Task<IActionResult> CloseSession(Guid id, [FromBody] ClosePOSSessionDto dto)
    {
        try
        {
            if (id != dto.SessionId)
                return BadRequest(new { Error = "ID mismatch" });

            var tenantId = GetTenantIdFromContext();
            var userId = GetUserIdFromContext();
            var result = await _service.CloseSessionAsync(dto, tenantId, userId);
            if (!result)
                return NotFound();
            return Ok(new { Message = "POS session closed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing POS session {SessionId}", id);
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

