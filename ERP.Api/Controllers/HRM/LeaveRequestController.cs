using ERP.Api.Security;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.HRM;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.HRM;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/leave-requests")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class LeaveRequestController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ILeaveRequestService _leaveRequestService;

    public LeaveRequestController(ILeaveRequestService leaveRequestService, ILogger<LeaveRequestController> logger) : base(logger)
    {
        _leaveRequestService = leaveRequestService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(IdResponse), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateLeaveRequestDto dto)
    {
        dto.TenantId = GetTenantId();
        var result = await _leaveRequestService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result }, new IdResponse { Id = result });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LeaveRequestDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var leaveRequest = await _leaveRequestService.GetByIdAsync(id, GetTenantId());
        if (leaveRequest == null)
            return NotFound();
        return Ok(leaveRequest);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LeaveRequestDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string? status = null)
    {
        var tenantId = GetTenantId();
        IEnumerable<LeaveRequestDto> leaveRequests;

        if (employeeId.HasValue)
            leaveRequests = await _leaveRequestService.GetByEmployeeAsync(employeeId.Value, tenantId);
        else if (!string.IsNullOrEmpty(status))
            leaveRequests = await _leaveRequestService.GetByStatusAsync(status, tenantId);
        else
            leaveRequests = await _leaveRequestService.GetAllAsync(tenantId);

        return Ok(leaveRequests);
    }

    [HttpPut("{id}/approve")]
    [ProducesResponseType(typeof(SuccessResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _leaveRequestService.ApproveAsync(id, GetTenantId(), GetUserId());
        return Ok(new SuccessResponse { Success = result });
    }

    [HttpPut("{id}/reject")]
    [ProducesResponseType(typeof(SuccessResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] LeaveRejectionDto? dto = null)
    {
        var result = await _leaveRequestService.RejectAsync(id, GetTenantId(), GetUserId(), dto?.Reason);
        return Ok(new SuccessResponse { Success = result });
    }

    [HttpPut("{id}/cancel")]
    [ProducesResponseType(typeof(bool), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] LeaveCancellationDto dto)
    {
        var result = await _leaveRequestService.CancelAsync(id, dto, GetTenantId(), GetUserId());
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _leaveRequestService.DeleteAsync(id, GetTenantId());
        if (!result)
            return NotFound();
        return NoContent();
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
