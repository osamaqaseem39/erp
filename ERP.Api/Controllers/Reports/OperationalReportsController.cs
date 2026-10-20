using ERP.Api.Abstractions;
using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Interfaces.Finance;
using ERP.Application.Interfaces.Reports;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.DTOs.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Reports;

[ApiController]
[Route("api/reports/operational")]
[Authorize]
[PermissionResource("reports.operational")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class OperationalReportsController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IOperationalReportService _reports;
    private readonly IInvoiceService _invoices;

    public OperationalReportsController(
        IOperationalReportService reports,
        IInvoiceService invoices,
        ILogger<OperationalReportsController> logger) : base(logger)
    {
        _reports = reports;
        _invoices = invoices;
    }

    [HttpGet("sales/pending-orders")]
    [ProducesResponseType(typeof(PendingSalesOrderReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> PendingSalesOrders([FromQuery] DateTime? asOfDate = null) =>
        Ok(await _reports.GetPendingSalesOrdersAsync(HttpContext.RequireTenantId(), asOfDate));

    [HttpGet("sales/invoice-summary")]
    [ProducesResponseType(typeof(SalesInvoiceSummaryReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> SalesInvoiceSummary(
        [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate) =>
        Ok(await _reports.GetSalesInvoiceSummaryAsync(HttpContext.RequireTenantId(), fromDate, toDate));

    [HttpGet("sales/summary-by-item")]
    [ProducesResponseType(typeof(SalesSummaryByItemReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> SalesSummaryByItem(
        [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate) =>
        Ok(await _reports.GetSalesSummaryByItemAsync(HttpContext.RequireTenantId(), fromDate, toDate));

    [HttpGet("sales/summary-customer-item")]
    [ProducesResponseType(typeof(SalesCustomerItemReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> SalesSummaryCustomerItem(
        [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate) =>
        Ok(await _reports.GetSalesSummaryCustomerItemAsync(HttpContext.RequireTenantId(), fromDate, toDate));

    [HttpGet("sales/summary-item-customer")]
    [ProducesResponseType(typeof(SalesCustomerItemReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> SalesSummaryItemCustomer(
        [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate) =>
        Ok(await _reports.GetSalesSummaryItemCustomerAsync(HttpContext.RequireTenantId(), fromDate, toDate));

    [HttpGet("sales/item-rate-detail")]
    [ProducesResponseType(typeof(SalesItemRateDetailReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> SalesItemRateDetail(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] Guid? itemId = null,
        [FromQuery] Guid? customerId = null) =>
        Ok(await _reports.GetSalesItemRateDetailAsync(
            HttpContext.RequireTenantId(), fromDate, toDate, itemId, customerId));

    [HttpGet("sales/customers")]
    [ProducesResponseType(typeof(CustomerInfoReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> CustomerInformation([FromQuery] string? status = null) =>
        Ok(await _reports.GetCustomerInformationAsync(HttpContext.RequireTenantId(), status));

    [HttpGet("sales/credit-notes")]
    [ProducesResponseType(typeof(CreditNoteReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> CreditNotes(
        [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate) =>
        Ok(await _reports.GetCreditNoteReportAsync(HttpContext.RequireTenantId(), fromDate, toDate));

    [HttpGet("sales/aging")]
    [ProducesResponseType(typeof(AgingReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> SaleAging(
        [FromQuery] DateTime asOfDate, [FromQuery] int[]? agingBuckets = null) =>
        Ok(await _invoices.GetAgedReceivablesAsync(HttpContext.RequireTenantId(), asOfDate, agingBuckets));

    [HttpGet("purchase/invoice-summary")]
    [ProducesResponseType(typeof(PurchaseInvoiceSummaryReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> PurchaseInvoiceSummary(
        [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate) =>
        Ok(await _reports.GetPurchaseInvoiceSummaryAsync(HttpContext.RequireTenantId(), fromDate, toDate));

    [HttpGet("stock/balances")]
    [ProducesResponseType(typeof(StockBalanceReportDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> StockBalances([FromQuery] Guid? warehouseId = null) =>
        Ok(await _reports.GetStockBalanceReportAsync(HttpContext.RequireTenantId(), warehouseId));
}
