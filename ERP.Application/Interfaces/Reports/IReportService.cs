using ERP.Shared.DTOs.Reports;

namespace ERP.Application.Interfaces.Reports;

public interface IReportService
{
    Task<ReportDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ReportDto>> GetAllAsync(Guid tenantId, string? category = null, bool? isActive = null);
    Task<IEnumerable<ReportDto>> GetByCategoryAsync(string category, Guid tenantId);
    Task<IEnumerable<ReportDto>> GetActiveAsync(Guid tenantId);
    Task<ReportDto> CreateAsync(CreateReportDto dto, Guid tenantId, Guid userId);
    Task<ReportDto> UpdateAsync(UpdateReportDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<ReportExecutionDto> ExecuteAsync(Guid reportId, Dictionary<string, object>? parameters, Guid tenantId, Guid userId);
    Task<IEnumerable<ReportExecutionDto>> GetExecutionsAsync(Guid reportId, Guid tenantId);
    Task<ReportExecutionDto?> GetExecutionByIdAsync(Guid executionId, Guid tenantId);
    
    // Additional methods for report management
    Task<IEnumerable<ReportDto>> GetAvailableReportsAsync(Guid tenantId);
    Task<ReportExecutionDto> GenerateReportAsync(GenerateReportDto dto, Guid tenantId, Guid userId);
    Task<IEnumerable<ReportScheduleDto>> GetScheduledReportsAsync(Guid tenantId);
    Task<ReportScheduleDto> ScheduleReportAsync(ScheduleReportDto dto, Guid tenantId, Guid userId);
    Task<ReportScheduleDto?> GetScheduledReportByIdAsync(Guid id, Guid tenantId);
    Task<bool> CancelScheduledReportAsync(Guid id, Guid tenantId);
    /// <summary>Runs all due active schedules (NextRunTime &lt;= now). Returns count executed.</summary>
    Task<int> ExecuteDueSchedulesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ReportExecutionDto>> GetReportHistoryAsync(Guid tenantId, string? reportType = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<ReportExportDto> ExportReportAsync(Guid reportId, Guid tenantId, string format = "json");
    
    // Financial report methods
    Task<object> GetTrialBalanceAsync(Guid tenantId, DateTime fromDate, DateTime toDate, string? currencyCode = null);
    Task<object> GetProfitAndLossAsync(Guid tenantId, DateTime fromDate, DateTime toDate, string? currencyCode = null);
    Task<object> GetBalanceSheetAsync(Guid tenantId, DateTime asOfDate, string? currencyCode = null);
    Task<object> GetCashFlowAsync(Guid tenantId, DateTime fromDate, DateTime toDate, string? currencyCode = null);
    Task<object> GetAgedReceivablesAsync(Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null);
    Task<object> GetAgedPayablesAsync(Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null);
    Task<object> GetGeneralLedgerAsync(Guid accountId, Guid tenantId, DateTime fromDate, DateTime toDate);
    Task<object> GetBudgetVsActualAsync(Guid tenantId, DateTime fromDate, DateTime toDate, Guid? budgetId = null);
}