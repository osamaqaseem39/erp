using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Api.Abstractions;
using ERP.Api.OpenApi;
using ERP.Application.Interfaces.Sales;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Sales;

[ApiController]
[Route("api/sales/pos-sessions")]
[Authorize]
[PermissionResource("sales.posSessions")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class POSSessionController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IPOSSessionService _service;

    public POSSessionController(IPOSSessionService service, ILogger<POSSessionController> logger) : base(logger)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreatePOSSessionDto dto)
    {
        var result = await _service.CreateAsync(dto, GetTenantIdFromContext(), GetUserIdFromContext());
        return CreatedAtAction(nameof(GetById), new { id = result }, new { SessionId = result });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(POSSessionDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var session = await _service.GetByIdAsync(id, GetTenantIdFromContext());
        if (session == null) return NotFound();
        return Ok(session);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<POSSessionDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? cashierId = null, [FromQuery] string? status = null)
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

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePOSSessionDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.ValidationError, Message = "ID mismatch" });
        var result = await _service.UpdateAsync(dto, GetTenantIdFromContext(), GetUserIdFromContext());
        if (!result) return NotFound();
        return Ok(new MessageResponse { Message = "POS session updated successfully" });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id, GetTenantIdFromContext());
        if (!result) return NotFound();
        return Ok(new MessageResponse { Message = "POS session deleted successfully" });
    }

    [HttpPost("{id}/close")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> CloseSession(Guid id, [FromBody] ClosePOSSessionDto dto)
    {
        if (id != dto.SessionId)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.ValidationError, Message = "ID mismatch" });
        var result = await _service.CloseSessionAsync(dto, GetTenantIdFromContext(), GetUserIdFromContext());
        if (!result) return NotFound();
        return Ok(new MessageResponse { Message = "POS session closed successfully" });
    }

    private Guid GetTenantIdFromContext() => HttpContext.RequireTenantId();
    private Guid GetUserIdFromContext() => User.RequireUserId();
}
