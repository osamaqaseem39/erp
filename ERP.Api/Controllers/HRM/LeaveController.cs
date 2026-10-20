using ERP.Api.Security;
using ERP.Api.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.HRM;
using ERP.Api.Validators.HRM;
using ERP.Application.Interfaces.HRM;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/leaves")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
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
    [ProducesResponseType(typeof(LeaveRequestDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var dto = await _service.GetLeaveRequestByIdAsync(id, GetTenantId());
        if (dto == null) return NotFound();
        return Ok(dto);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LeaveRequestDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll()
        => await HandleAsync(() => _service.GetPendingLeaveRequestsAsync(GetTenantId()));

    [HttpGet("employee/{employeeId}")]
    [ProducesResponseType(typeof(IEnumerable<LeaveRequestDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByEmployee(Guid employeeId)
        => await HandleAsync(() => _service.GetLeaveRequestsByEmployeeAsync(employeeId, GetTenantId()));

    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<LeaveRequestDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByStatus(string status)
        => await HandleAsync(() => _service.GetLeaveRequestsByStatusAsync(status, GetTenantId()));

    [HttpPost]
    [ProducesResponseType(typeof(LeaveRequestDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Create([FromBody] CreateLeaveRequestDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        return await HandleAsync(() => _service.AddLeaveRequestAsync(dto, GetTenantId(), GetUserId()));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(LeaveRequestDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeaveRequestDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        return await HandleAsync(() => _service.UpdateLeaveRequestAsync(dto, GetTenantId()));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteLeaveRequestAsync(id, GetTenantId());
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/approve")]
    [ProducesResponseType(typeof(LeaveRequestDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveLeaveRequestDto? dto = null)
    {
        var approvalDto = new LeaveApprovalDto
        {
            LeaveRequestId = id,
            Notes = dto?.Comments
        };
        return await HandleAsync(() => _service.ApproveLeaveAsync(approvalDto, GetTenantId(), GetUserId()));
    }

    [HttpPost("{id}/reject")]
    [ProducesResponseType(typeof(LeaveRequestDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectLeaveRequestDto? dto = null)
    {
        dto ??= new RejectLeaveRequestDto();
        dto.LeaveRequestId = id;
        return await HandleAsync(() => _service.RejectLeaveAsync(dto, GetTenantId(), GetUserId()));
    }

    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(LeaveRequestDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] LeaveCancellationDto? dto = null)
        => await HandleAsync(() => _service.CancelLeaveAsync(id, dto?.Reason, GetTenantId(), GetUserId()));

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
