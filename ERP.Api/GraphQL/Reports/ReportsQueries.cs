using ERP.Application.Interfaces.Finance;
using ERP.Application.Interfaces.Reports;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.DTOs.Reports;
using HotChocolate;

namespace ERP.Api.GraphQL.Reports;

[ExtendObjectType("Query")]
public class ReportsQueries
{
    // Report Queries
    public async Task<ReportDto?> GetReport(
        [Service] IReportService service,
        Guid reportId,
        Guid tenantId)
    {
        return await service.GetByIdAsync(reportId, tenantId);
    }

    public async Task<IEnumerable<ReportDto>> GetReports(
        [Service] IReportService service,
        Guid tenantId,
        string? category = null,
        bool? isActive = null)
    {
        return await service.GetAllAsync(tenantId, category, isActive);
    }

    public async Task<IEnumerable<ReportDto>> GetReportsByCategory(
        [Service] IReportService service,
        string category,
        Guid tenantId)
    {
        return await service.GetByCategoryAsync(category, tenantId);
    }

    public async Task<IEnumerable<ReportDto>> GetActiveReports(
        [Service] IReportService service,
        Guid tenantId)
    {
        return await service.GetActiveAsync(tenantId);
    }

    public async Task<IEnumerable<ReportDto>> GetAvailableReports(
        [Service] IReportService service,
        Guid tenantId)
    {
        return await service.GetAvailableReportsAsync(tenantId);
    }

    // Report Execution Queries
    public async Task<ReportExecutionDto?> GetReportExecution(
        [Service] IReportService service,
        Guid executionId,
        Guid tenantId)
    {
        return await service.GetExecutionByIdAsync(executionId, tenantId);
    }

    public async Task<IEnumerable<ReportExecutionDto>> GetReportExecutions(
        [Service] IReportService service,
        Guid reportId,
        Guid tenantId)
    {
        return await service.GetExecutionsAsync(reportId, tenantId);
    }

    public async Task<IEnumerable<ReportExecutionDto>> GetReportHistory(
        [Service] IReportService service,
        Guid tenantId,
        string? reportType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        return await service.GetReportHistoryAsync(tenantId, reportType, fromDate, toDate);
    }

    // Report Schedule Queries
    public async Task<IEnumerable<ReportScheduleDto>> GetScheduledReports(
        [Service] IReportService service,
        Guid tenantId)
    {
        return await service.GetScheduledReportsAsync(tenantId);
    }

    public async Task<ReportScheduleDto?> GetScheduledReport(
        [Service] IReportService service,
        Guid scheduleId,
        Guid tenantId)
    {
        return await service.GetScheduledReportByIdAsync(scheduleId, tenantId);
    }

    // Financial Report Queries
    public async Task<object> GetTrialBalance(
        [Service] IReportService service,
        Guid tenantId,
        DateTime fromDate,
        DateTime toDate,
        string? currencyCode = null)
    {
        return await service.GetTrialBalanceAsync(tenantId, fromDate, toDate, currencyCode);
    }

    public async Task<object> GetProfitAndLoss(
        [Service] IReportService service,
        Guid tenantId,
        DateTime fromDate,
        DateTime toDate,
        string? currencyCode = null)
    {
        return await service.GetProfitAndLossAsync(tenantId, fromDate, toDate, currencyCode);
    }

    public async Task<object> GetBalanceSheet(
        [Service] IReportService service,
        Guid tenantId,
        DateTime asOfDate,
        string? currencyCode = null)
    {
        return await service.GetBalanceSheetAsync(tenantId, asOfDate, currencyCode);
    }

    public async Task<object> GetCashFlow(
        [Service] IReportService service,
        Guid tenantId,
        DateTime fromDate,
        DateTime toDate,
        string? currencyCode = null)
    {
        return await service.GetCashFlowAsync(tenantId, fromDate, toDate, currencyCode);
    }

    // Operational reports (REST parity: /api/reports/operational/*)
    public async Task<PendingSalesOrderReportDto> GetPendingSalesOrdersReport(
        [Service] IOperationalReportService service,
        Guid tenantId,
        DateTime? asOfDate = null)
        => await service.GetPendingSalesOrdersAsync(tenantId, asOfDate);

    public async Task<SalesInvoiceSummaryReportDto> GetSalesInvoiceSummaryReport(
        [Service] IOperationalReportService service,
        Guid tenantId,
        DateTime fromDate,
        DateTime toDate)
        => await service.GetSalesInvoiceSummaryAsync(tenantId, fromDate, toDate);

    public async Task<SalesSummaryByItemReportDto> GetSalesSummaryByItemReport(
        [Service] IOperationalReportService service,
        Guid tenantId,
        DateTime fromDate,
        DateTime toDate)
        => await service.GetSalesSummaryByItemAsync(tenantId, fromDate, toDate);

    public async Task<SalesCustomerItemReportDto> GetSalesSummaryCustomerItemReport(
        [Service] IOperationalReportService service,
        Guid tenantId,
        DateTime fromDate,
        DateTime toDate)
        => await service.GetSalesSummaryCustomerItemAsync(tenantId, fromDate, toDate);

    public async Task<SalesCustomerItemReportDto> GetSalesSummaryItemCustomerReport(
        [Service] IOperationalReportService service,
        Guid tenantId,
        DateTime fromDate,
        DateTime toDate)
        => await service.GetSalesSummaryItemCustomerAsync(tenantId, fromDate, toDate);

    public async Task<SalesItemRateDetailReportDto> GetSalesItemRateDetailReport(
        [Service] IOperationalReportService service,
        Guid tenantId,
        DateTime fromDate,
        DateTime toDate,
        Guid? itemId = null,
        Guid? customerId = null)
        => await service.GetSalesItemRateDetailAsync(tenantId, fromDate, toDate, itemId, customerId);

    public async Task<CustomerInfoReportDto> GetCustomerInformationReport(
        [Service] IOperationalReportService service,
        Guid tenantId,
        string? status = null)
        => await service.GetCustomerInformationAsync(tenantId, status);

    public async Task<CreditNoteReportDto> GetCreditNoteReport(
        [Service] IOperationalReportService service,
        Guid tenantId,
        DateTime fromDate,
        DateTime toDate)
        => await service.GetCreditNoteReportAsync(tenantId, fromDate, toDate);

    public async Task<PurchaseInvoiceSummaryReportDto> GetPurchaseInvoiceSummaryReport(
        [Service] IOperationalReportService service,
        Guid tenantId,
        DateTime fromDate,
        DateTime toDate)
        => await service.GetPurchaseInvoiceSummaryAsync(tenantId, fromDate, toDate);

    public async Task<StockBalanceReportDto> GetStockBalanceReport(
        [Service] IOperationalReportService service,
        Guid tenantId,
        Guid? warehouseId = null)
        => await service.GetStockBalanceReportAsync(tenantId, warehouseId);

    public async Task<AgingReportDto> GetSaleAgingReport(
        [Service] IInvoiceService invoices,
        Guid tenantId,
        DateTime asOfDate,
        int[]? agingBuckets = null)
        => await invoices.GetAgedReceivablesAsync(tenantId, asOfDate, agingBuckets);
}

