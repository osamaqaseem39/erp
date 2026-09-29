using ERP.Api.Security;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.HRM;
using ERP.Shared.DTOs.HRM;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/attendance")]
[Authorize]
public class AttendanceController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IAttendanceService _attendanceService;
    private readonly ILogger<AttendanceController> _logger;

    public AttendanceController(IAttendanceService attendanceService, ILogger<AttendanceController> logger) : base(logger)
    {
        _attendanceService = attendanceService;
        _logger = logger;
    }

    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _attendanceService.CheckInAsync(dto, tenantId, userId);
        return Ok(result);
    }

    [HttpPost("check-out")]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _attendanceService.CheckOutAsync(dto, tenantId, userId);
        return Ok(result);
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetEmployeeAttendance(Guid employeeId, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var tenantId = GetTenantId();
        var from = fromDate ?? DateTime.Today.AddDays(-30);
        var to = toDate ?? DateTime.Today;
        
        var attendance = await _attendanceService.GetEmployeeAttendanceAsync(employeeId, tenantId, from, to);
        return Ok(attendance);
    }

    [HttpGet("summary/{employeeId}")]
    public async Task<IActionResult> GetAttendanceSummary(Guid employeeId, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var tenantId = GetTenantId();
        var from = fromDate ?? DateTime.Today.AddDays(-30);
        var to = toDate ?? DateTime.Today;
        
        var summary = await _attendanceService.GetAttendanceSummaryAsync(employeeId, tenantId, from, to);
        return Ok(summary);
    }

    [HttpPut("adjust")]
    public async Task<IActionResult> AdjustAttendance([FromBody] AttendanceAdjustmentDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _attendanceService.AdjustAttendanceAsync(dto, tenantId, userId);
        return Ok(result);
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();

    private Guid GetUserId() => User.RequireUserId();
}