using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.HRM;
using ERP.Shared.DTOs.HRM;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/leave-requests")]
[Authorize]
public class LeaveRequestController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ILeaveRequestService _leaveRequestService;
    private readonly ILogger<LeaveRequestController> _logger;

    public LeaveRequestController(ILeaveRequestService leaveRequestService, ILogger<LeaveRequestController> logger) : base(logger)
    {
        _leaveRequestService = leaveRequestService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLeaveRequestDto dto)
    {
        var result = await _leaveRequestService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result }, new { LeaveRequestId = result });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var leaveRequest = await _leaveRequestService.GetByIdAsync(id);
        if (leaveRequest == null)
            return NotFound();
        return Ok(leaveRequest);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? employeeId = null, [FromQuery] string? status = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        IEnumerable<LeaveRequestDto> leaveRequests;
        
        if (employeeId.HasValue)
            leaveRequests = await _leaveRequestService.GetByEmployeeAsync(employeeId.Value);
        else if (!string.IsNullOrEmpty(status))
            leaveRequests = await _leaveRequestService.GetByStatusAsync(status);
        else
            leaveRequests = await _leaveRequestService.GetAllAsync();
            
        return Ok(leaveRequests);
    }

    [HttpPut("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] LeaveApprovalDto dto)
    {
        var result = await _leaveRequestService.ApproveAsync(id);
        return Ok(new { Success = result });
    }

    [HttpPut("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] LeaveRejectionDto dto)
    {
        var result = await _leaveRequestService.RejectAsync(id);
        return Ok(new { Success = result });
    }

    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] LeaveCancellationDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _leaveRequestService.CancelAsync(id, dto, tenantId, userId);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _leaveRequestService.DeleteAsync(id);
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