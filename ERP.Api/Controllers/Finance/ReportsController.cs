using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Finance;
using ERP.Application.Interfaces.Reports;
using ERP.Shared.DTOs.Finance;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/reports")]
[Authorize]
public class ReportsController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IReportService _reportsService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportService reportsService, ILogger<ReportsController> logger) : base(logger)
    {
        _reportsService = reportsService;
        _logger = logger;
    }

    [HttpGet("trial-balance")]
    public async Task<IActionResult> GetTrialBalance([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string? currencyCode = null)
    {
        var tenantId = GetTenantId();
        var report = await _reportsService.GetTrialBalanceAsync(tenantId, fromDate, toDate, currencyCode);
        return Ok(report);
    }

    [HttpGet("profit-loss")]
    public async Task<IActionResult> GetProfitAndLoss([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string? currencyCode = null)
    {
        var tenantId = GetTenantId();
        var report = await _reportsService.GetProfitAndLossAsync(tenantId, fromDate, toDate, currencyCode);
        return Ok(report);
    }

    [HttpGet("balance-sheet")]
    public async Task<IActionResult> GetBalanceSheet([FromQuery] DateTime asOfDate, [FromQuery] string? currencyCode = null)
    {
        var tenantId = GetTenantId();
        var report = await _reportsService.GetBalanceSheetAsync(tenantId, asOfDate, currencyCode);
        return Ok(report);
    }

    [HttpGet("cash-flow")]
    public async Task<IActionResult> GetCashFlow([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string? currencyCode = null)
    {
        var tenantId = GetTenantId();
        var report = await _reportsService.GetCashFlowAsync(tenantId, fromDate, toDate, currencyCode);
        return Ok(report);
    }

    [HttpGet("aged-receivables")]
    public async Task<IActionResult> GetAgedReceivables([FromQuery] DateTime asOfDate, [FromQuery] int[]? agingBuckets = null)
    {
        var tenantId = GetTenantId();
        var report = await _reportsService.GetAgedReceivablesAsync(tenantId, asOfDate, agingBuckets);
        return Ok(report);
    }

    [HttpGet("aged-payables")]
    public async Task<IActionResult> GetAgedPayables([FromQuery] DateTime asOfDate, [FromQuery] int[]? agingBuckets = null)
    {
        var tenantId = GetTenantId();
        var report = await _reportsService.GetAgedPayablesAsync(tenantId, asOfDate, agingBuckets);
        return Ok(report);
    }

    [HttpGet("general-ledger")]
    public async Task<IActionResult> GetGeneralLedger([FromQuery] Guid accountId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var tenantId = GetTenantId();
        var report = await _reportsService.GetGeneralLedgerAsync(accountId, tenantId, fromDate, toDate);
        return Ok(report);
    }

    [HttpGet("budget-vs-actual")]
    public async Task<IActionResult> GetBudgetVsActual([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] Guid? budgetId = null)
    {
        var tenantId = GetTenantId();
        var report = await _reportsService.GetBudgetVsActualAsync(tenantId, fromDate, toDate, budgetId);
        return Ok(report);
    }

    private Guid GetTenantId()
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (Guid.TryParse(tenantIdClaim, out var tenantId))
            return tenantId;
        throw new UnauthorizedAccessException("Tenant ID not found in token");
    }
}