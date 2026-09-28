using ERP.Application.Interfaces.Reports;
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
}

