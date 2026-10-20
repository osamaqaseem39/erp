using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Api.OpenApi;
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
[PermissionResource("finance.reports")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
public class ReportsController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IAccountingService _accounting;
    private readonly IInvoiceService _invoices;
    private readonly IReportService _reportsService;

    public ReportsController(
        IAccountingService accounting,
        IInvoiceService invoices,
        IReportService reportsService,
        ILogger<ReportsController> logger) : base(logger)
    {
        _accounting = accounting;
        _invoices = invoices;
        _reportsService = reportsService;
    }

    [HttpGet("trial-balance")]
    [ProducesResponseType(typeof(TrialBalanceReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetTrialBalance(
        [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string? currencyCode = null)
    {
        var report = await _accounting.GetTrialBalanceAsync(GetTenantId(), fromDate, toDate, currencyCode);
        return Ok(report);
    }

    [HttpGet("running-trial-balance")]
    [ProducesResponseType(typeof(RunningTrialBalanceReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetRunningTrialBalance(
        [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string? currencyCode = null)
    {
        var report = await _accounting.GetRunningTrialBalanceAsync(GetTenantId(), fromDate, toDate, currencyCode);
        return Ok(report);
    }

    [HttpGet("profit-loss")]
    [ProducesResponseType(typeof(ProfitAndLossReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetProfitAndLoss(
        [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string? currencyCode = null)
    {
        var report = await _accounting.GetProfitAndLossAsync(GetTenantId(), fromDate, toDate, currencyCode);
        return Ok(report);
    }

    [HttpGet("balance-sheet")]
    [ProducesResponseType(typeof(BalanceSheetReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetBalanceSheet(
        [FromQuery] DateTime asOfDate, [FromQuery] string? currencyCode = null)
    {
        var report = await _accounting.GetBalanceSheetAsync(GetTenantId(), asOfDate, currencyCode);
        return Ok(report);
    }

    [HttpGet("general-ledger")]
    [ProducesResponseType(typeof(LedgerReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetGeneralLedger(
        [FromQuery] Guid accountId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] bool postingOnly = false)
    {
        var report = await _accounting.GetGeneralLedgerBalanceAsync(
            accountId, GetTenantId(), fromDate, toDate, postingOnly);
        return Ok(report);
    }

    [HttpGet("vouchers")]
    [ProducesResponseType(typeof(VoucherReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetVoucherReport(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? journalType = null,
        [FromQuery] string? status = null)
    {
        var report = await _accounting.GetVoucherReportAsync(
            GetTenantId(), fromDate, toDate, journalType, status);
        return Ok(report);
    }

    [HttpGet("chart-of-accounts")]
    [ProducesResponseType(typeof(ChartOfAccountReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetChartOfAccountReport([FromQuery] bool includeDetailAccounts = true)
    {
        var report = await _accounting.GetChartOfAccountReportAsync(GetTenantId(), includeDetailAccounts);
        return Ok(report);
    }

    [HttpGet("daily-financial")]
    [ProducesResponseType(typeof(DailyFinancialReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetDailyFinancial(
        [FromQuery] DateTime date, [FromQuery] string? currencyCode = null) =>
        Ok(await _accounting.GetDailyFinancialAsync(GetTenantId(), date, currencyCode));

    [HttpGet("cash-bank-vouchers")]
    [ProducesResponseType(typeof(CashBankVoucherSummaryReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetCashBankVoucherSummary(
        [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate) =>
        Ok(await _accounting.GetCashBankVoucherSummaryAsync(GetTenantId(), fromDate, toDate));

    [HttpGet("cash-bank-ledger")]
    [ProducesResponseType(typeof(CashBankLedgerSummaryReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetCashBankLedgerSummary(
        [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate) =>
        Ok(await _accounting.GetCashBankLedgerSummaryAsync(GetTenantId(), fromDate, toDate));

    [HttpGet("voucher-type-monthly")]
    [ProducesResponseType(typeof(VoucherTypeMonthlyReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetVoucherTypeMonthlySummary(
        [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate) =>
        Ok(await _accounting.GetVoucherTypeMonthlySummaryAsync(GetTenantId(), fromDate, toDate));

    [HttpGet("partner-balances")]
    [ProducesResponseType(typeof(PartnerBalanceSummaryReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetPartnerBalances([FromQuery] DateTime asOfDate) =>
        Ok(await _invoices.GetPartnerBalanceSummaryAsync(GetTenantId(), asOfDate));

    [HttpGet("aged-receivables")]
    [ProducesResponseType(typeof(AgingReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAgedReceivables(
        [FromQuery] DateTime asOfDate, [FromQuery] int[]? agingBuckets = null) =>
        Ok(await _invoices.GetAgedReceivablesAsync(GetTenantId(), asOfDate, agingBuckets));

    [HttpGet("aged-payables")]
    [ProducesResponseType(typeof(AgingReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAgedPayables(
        [FromQuery] DateTime asOfDate, [FromQuery] int[]? agingBuckets = null) =>
        Ok(await _invoices.GetAgedPayablesAsync(GetTenantId(), asOfDate, agingBuckets));

    [HttpGet("cash-flow")]
    [ProducesResponseType(typeof(CashFlowReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetCashFlow(
        [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string? currencyCode = null) =>
        Ok(await _accounting.GetCashFlowAsync(GetTenantId(), fromDate, toDate, currencyCode));

    [HttpGet("budget-vs-actual")]
    [ProducesResponseType(typeof(BudgetVsActualReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetBudgetVsActual(
        [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] Guid? budgetId = null) =>
        Ok(await _reportsService.GetBudgetVsActualAsync(GetTenantId(), fromDate, toDate, budgetId));

    [HttpGet("cheque-register")]
    [ProducesResponseType(typeof(ChequeRegisterReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetChequeRegister(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? chequeStatus = null,
        [FromQuery] string? journalType = null,
        [FromQuery] string? search = null) =>
        Ok(await _accounting.GetChequeRegisterAsync(
            GetTenantId(), fromDate, toDate, chequeStatus, journalType, search));

    private Guid GetTenantId() => HttpContext.RequireTenantId();
}
