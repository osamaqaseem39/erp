using ERP.Api.Security.Permissions;
using ERP.Api.Security;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Reports;
using ERP.Shared.DTOs.Reports;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Reports;

[ApiController]
[Route("api/reports")]
[Authorize]
[PermissionResource("reports.reports")]
public class ReportController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportController> _logger;

    public ReportController(IReportService reportService, ILogger<ReportController> logger) : base(logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableReports()
    {
        var tenantId = GetTenantId();
        var reports = await _reportService.GetAvailableReportsAsync(tenantId);
        return Ok(reports);
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateReport([FromBody] GenerateReportDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _reportService.GenerateReportAsync(dto, tenantId, userId);
        return Ok(result);
    }

    [HttpGet("scheduled")]
    public async Task<IActionResult> GetScheduledReports()
    {
        var tenantId = GetTenantId();
        var reports = await _reportService.GetScheduledReportsAsync(tenantId);
        return Ok(reports);
    }

    [HttpPost("schedule")]
    public async Task<IActionResult> ScheduleReport([FromBody] ScheduleReportDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _reportService.ScheduleReportAsync(dto, tenantId, userId);
        return CreatedAtAction(nameof(GetScheduledReport), new { id = result.Id }, result);
    }

    [HttpGet("schedule/{id}")]
    public async Task<IActionResult> GetScheduledReport(Guid id)
    {
        var tenantId = GetTenantId();
        var report = await _reportService.GetScheduledReportByIdAsync(id, tenantId);
        if (report == null)
            return NotFound();
        return Ok(report);
    }

    [HttpDelete("schedule/{id}")]
    public async Task<IActionResult> CancelScheduledReport(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _reportService.CancelScheduledReportAsync(id, tenantId);
        if (!result)
            return NotFound();
        return NoContent();
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetReportHistory([FromQuery] string? reportType = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var tenantId = GetTenantId();
        var history = await _reportService.GetReportHistoryAsync(tenantId, reportType, fromDate, toDate);
        return Ok(history);
    }

    [HttpGet("export/{reportId}")]
    public async Task<IActionResult> ExportReport(Guid reportId, [FromQuery] string format = "pdf")
    {
        var tenantId = GetTenantId();
        var result = await _reportService.ExportReportAsync(reportId, tenantId, format);
        return File(result.Content, result.ContentType, result.FileName);
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();

    private Guid GetUserId() => User.RequireUserId();
}