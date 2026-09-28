using ERP.Api.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.Api.Abstractions;
using ERP.Api.Services;
using ERP.Shared.DTOs.HRM;
using FluentValidation;
using ERP.Api.Validators.HRM;
using ERP.Application.Interfaces.HRM;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/leaves")]
[Authorize]
public class LeaveController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ILeaveService _service;
    private readonly CreateLeaveRequestValidator _createValidator;
    private readonly UpdateLeaveRequestValidator _updateValidator;

    public LeaveController(
        ILeaveService service,
        CreateLeaveRequestValidator createValidator,
        UpdateLeaveRequestValidator updateValidator,
        ILogger<LeaveController> logger) : base(logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return await HandleAsync(() => _service.GetLeaveRequestByIdAsync(id));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return await HandleAsync(() => _service.GetPendingLeaveRequestsAsync());
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetByEmployee(Guid employeeId)
    {
        return await HandleAsync(() => _service.GetLeaveRequestsByEmployeeAsync(employeeId));
    }

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(string status)
    {
        // Note: ILeaveService doesn't have GetByStatusAsync method
        // This would need to be implemented or use a different approach
        return await HandleAsync(() => _service.GetPendingLeaveRequestsAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLeaveRequestDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));
        }

        return await HandleAsync(() => _service.AddLeaveRequestAsync(dto));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeaveRequestDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));
        }

        return await HandleAsync(() => _service.UpdateLeaveRequestAsync(dto));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await HandleAsync(() => _service.DeleteLeaveRequestAsync(id));
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveLeaveRequestDto dto)
    {
        var approvalDto = new LeaveApprovalDto
        {
            LeaveRequestId = id,
            Notes = dto.Comments
        };
        var approverId = GetUserId(); // Get from claims
        return await HandleAsync(() => _service.ApproveLeaveAsync(approvalDto, approverId));
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectLeaveRequestDto dto)
    {
        // Note: ILeaveService doesn't have RejectAsync method
        // This would need to be implemented or use a different approach
        return BadRequest(new { Error = "Reject functionality not yet implemented" });
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;
        throw new UnauthorizedAccessException("User ID not found in token");
    }
}