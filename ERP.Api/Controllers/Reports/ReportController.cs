using ERP.Api.Security.Permissions;
using ERP.Api.Security;
using ERP.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Reports;
using ERP.Shared.DTOs.Reports;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;

namespace ERP.Api.Controllers.Reports;

[ApiController]
[Route("api/reports")]
[Authorize]
[PermissionResource("reports.reports")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class ReportController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IEffectivePermissions _permissions;
    private readonly ILogger<ReportController> _logger;

    public ReportController(
        IReportService reportService,
        IEffectivePermissions permissions,
        ILogger<ReportController> logger) : base(logger)
    {
        _reportService = reportService;
        _permissions = permissions;
        _logger = logger;
    }

    [HttpGet("available")]
    [ProducesResponseType(typeof(IEnumerable<ReportDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAvailableReports()
    {
        var tenantId = GetTenantId();
        var reports = (await _reportService.GetAvailableReportsAsync(tenantId)).ToList();
        if (User.IsSuperAdmin()) return Ok(reports);

        var held = await _permissions.GetAsync(tenantId, GetUserId());
        // Users with the coarse reports.reports.view see the full list; otherwise filter by per-report codes.
        if (held.Contains("reports.reports.view")) return Ok(reports);

        var filtered = reports.Where(r =>
        {
            if (!string.IsNullOrWhiteSpace(r.ReportCode))
            {
                var byCode = ERP.Application.Reports.ReportCatalog.ViewPermission(r.ReportCode);
                if (held.Contains(byCode)) return true;
            }
            var code = ReportViewPermissionCode(r.ReportCode) ?? ReportViewPermissionCode(r.Name) ?? ReportViewPermissionCode(r.ReportName);
            return code != null && held.Contains(code);
        });
        return Ok(filtered);
    }

    /// <summary>Maps a report code/name to a seeded permission, e.g. "Stock Average" → reports.stockAverage.view.</summary>
    internal static string? ReportViewPermissionCode(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        if (name.Contains('_'))
            return ERP.Application.Reports.ReportCatalog.ViewPermission(name);
        var camel = Regex.Replace(name.Trim(), @"[^A-Za-z0-9]+", " ");
        var parts = camel.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return null;
        var resource = parts[0].ToLowerInvariant() + string.Concat(parts.Skip(1).Select(p =>
            char.ToUpperInvariant(p[0]) + p[1..].ToLowerInvariant()));
        return $"reports.{resource}.view";
    }

    [HttpPost("generate")]
    [ProducesResponseType(typeof(ReportExecutionDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GenerateReport([FromBody] GenerateReportDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _reportService.GenerateReportAsync(dto, tenantId, userId);
        return Ok(result);
    }

    [HttpGet("scheduled")]
    [ProducesResponseType(typeof(IEnumerable<ReportScheduleDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetScheduledReports()
    {
        var tenantId = GetTenantId();
        var reports = await _reportService.GetScheduledReportsAsync(tenantId);
        return Ok(reports);
    }

    [HttpPost("schedule")]
    [ProducesResponseType(typeof(ReportScheduleDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> ScheduleReport([FromBody] ScheduleReportDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _reportService.ScheduleReportAsync(dto, tenantId, userId);
        return CreatedAtAction(nameof(GetScheduledReport), new { id = result.Id }, result);
    }

    [HttpGet("schedule/{id}")]
    [ProducesResponseType(typeof(ReportScheduleDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetScheduledReport(Guid id)
    {
        var tenantId = GetTenantId();
        var report = await _reportService.GetScheduledReportByIdAsync(id, tenantId);
        if (report == null)
            return NotFound();
        return Ok(report);
    }

    [HttpDelete("schedule/{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> CancelScheduledReport(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _reportService.CancelScheduledReportAsync(id, tenantId);
        if (!result)
            return NotFound();
        return NoContent();
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(IEnumerable<ReportExecutionDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetReportHistory([FromQuery] string? reportType = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var tenantId = GetTenantId();
        var history = await _reportService.GetReportHistoryAsync(tenantId, reportType, fromDate, toDate);
        return Ok(history);
    }

    [HttpGet("export/{reportId}")]
    [ProducesResponseType(typeof(FileContentResult), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ExportReport(Guid reportId, [FromQuery] string format = "json")
    {
        var tenantId = GetTenantId();
        try
        {
            var result = await _reportService.ExportReportAsync(reportId, tenantId, format);
            return File(result.Content, result.ContentType, result.FileName);
        }
        catch (NotSupportedException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();

    private Guid GetUserId() => User.RequireUserId();
}
