using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Reports;
using ERP.Shared.DTOs.Reports;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Reports;

[ApiController]
[Route("api/reports")]
[Authorize]
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
        try
        {
            var tenantId = GetTenantId();
            var reports = await _reportService.GetAvailableReportsAsync(tenantId);
            return Ok(reports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available reports");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateReport([FromBody] GenerateReportDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _reportService.GenerateReportAsync(dto, tenantId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating report");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("scheduled")]
    public async Task<IActionResult> GetScheduledReports()
    {
        try
        {
            var tenantId = GetTenantId();
            var reports = await _reportService.GetScheduledReportsAsync(tenantId);
            return Ok(reports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting scheduled reports");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("schedule")]
    public async Task<IActionResult> ScheduleReport([FromBody] ScheduleReportDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _reportService.ScheduleReportAsync(dto, tenantId, userId);
            return CreatedAtAction(nameof(GetScheduledReport), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling report");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("schedule/{id}")]
    public async Task<IActionResult> GetScheduledReport(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var report = await _reportService.GetScheduledReportByIdAsync(id, tenantId);
            if (report == null)
                return NotFound();
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting scheduled report {ScheduleId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("schedule/{id}")]
    public async Task<IActionResult> CancelScheduledReport(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _reportService.CancelScheduledReportAsync(id, tenantId);
            if (!result)
                return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling scheduled report {ScheduleId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetReportHistory([FromQuery] string? reportType = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var tenantId = GetTenantId();
            var history = await _reportService.GetReportHistoryAsync(tenantId, reportType, fromDate, toDate);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report history");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("export/{reportId}")]
    public async Task<IActionResult> ExportReport(Guid reportId, [FromQuery] string format = "pdf")
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _reportService.ExportReportAsync(reportId, tenantId, format);
            return File(result.Content, result.ContentType, result.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report {ReportId}", reportId);
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