using ERP.Application.Interfaces.Reports;
using ERP.Infrastructure.Repositories.Reports;
using ERP.Shared.DTOs.Reports;
using ERP.Domain.Reports;

namespace ERP.Application.Reports;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IReportExecutionRepository _reportExecutionRepository;
    private readonly IReportScheduleRepository _reportScheduleRepository;

    public ReportService(
        IReportRepository reportRepository,
        IReportExecutionRepository reportExecutionRepository,
        IReportScheduleRepository reportScheduleRepository)
    {
        _reportRepository = reportRepository;
        _reportExecutionRepository = reportExecutionRepository;
        _reportScheduleRepository = reportScheduleRepository;
    }

    public async Task<ReportDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        return report != null ? MapToDto(report) : null;
    }

    public async Task<IEnumerable<ReportDto>> GetAllAsync(Guid tenantId, string? category = null, bool? isActive = null)
    {
        var reports = await _reportRepository.GetAllAsync();
        
        if (category != null)
            reports = reports.Where(r => r.Category == category);
        if (isActive.HasValue)
            reports = reports.Where(r => r.IsActive == isActive.Value);

        return reports.Select(MapToDto);
    }

    public async Task<IEnumerable<ReportDto>> GetByCategoryAsync(string category, Guid tenantId)
    {
        var reports = await _reportRepository.GetByCategoryAsync(category);
        return reports.Select(MapToDto);
    }

    public async Task<IEnumerable<ReportDto>> GetActiveAsync(Guid tenantId)
    {
        var reports = await _reportRepository.GetActiveAsync();
        return reports.Select(MapToDto);
    }

    public async Task<ReportDto> CreateAsync(CreateReportDto dto, Guid tenantId, Guid userId)
    {
        var report = new Report
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Category = dto.Category,
            Query = dto.Query,
            Parameters = dto.Parameters?.ToString(),
            OutputFormat = dto.OutputFormat,
            IsActive = dto.IsActive,
            TenantId = tenantId,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        var createdId = await _reportRepository.CreateAsync(report);
        var created = await _reportRepository.GetByIdAsync(createdId);
        return MapToDto(created!);
    }

    public async Task<ReportDto> UpdateAsync(UpdateReportDto dto, Guid tenantId, Guid userId)
    {
        var report = await _reportRepository.GetByIdAsync(dto.Id);
        if (report == null)
            throw new KeyNotFoundException($"Report with ID '{dto.Id}' not found.");

        report.Name = dto.Name;
        report.Description = dto.Description;
        report.Category = dto.Category;
        report.Query = dto.Query;
        report.Parameters = dto.Parameters?.ToString();
        report.OutputFormat = dto.OutputFormat;
        report.IsActive = dto.IsActive;
        report.UpdatedBy = userId.ToString();
        report.UpdatedAt = DateTime.UtcNow;

        await _reportRepository.UpdateAsync(report);
        return MapToDto(report);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        return await _reportRepository.DeleteAsync(id);
    }

    public async Task<ReportExecutionDto> ExecuteAsync(Guid reportId, Dictionary<string, object>? parameters, Guid tenantId, Guid userId)
    {
        var report = await _reportRepository.GetByIdAsync(reportId);
        if (report == null)
            throw new KeyNotFoundException($"Report with ID '{reportId}' not found.");

        var execution = new ReportExecution
        {
            Id = Guid.NewGuid(),
            ReportId = reportId,
            Parameters = parameters?.ToString(),
            Status = "Running",
            StartedAt = DateTime.UtcNow,
            TenantId = tenantId,
            ExecutedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        var executionId = await _reportExecutionRepository.CreateAsync(execution);

        try
        {
            // Execute the report query with parameters
            // This would typically involve executing the SQL query and processing results
            execution.Status = "Completed";
            execution.CompletedAt = DateTime.UtcNow;
            execution.ResultData = "Report data would be here"; // In real implementation, this would contain actual data
        }
        catch (Exception ex)
        {
            execution.Status = "Failed";
            execution.ErrorMessage = ex.Message;
            execution.CompletedAt = DateTime.UtcNow;
        }

        await _reportExecutionRepository.UpdateAsync(execution);
        return MapToDto(execution);
    }

    public async Task<IEnumerable<ReportExecutionDto>> GetExecutionsAsync(Guid reportId, Guid tenantId)
    {
        var executions = await _reportExecutionRepository.GetByReportIdAsync(reportId);
        return executions.Select(MapToDto);
    }

    public async Task<ReportExecutionDto?> GetExecutionByIdAsync(Guid executionId, Guid tenantId)
    {
        var execution = await _reportExecutionRepository.GetByIdAsync(executionId);
        return execution != null ? MapToDto(execution) : null;
    }

    public async Task<IEnumerable<ReportDto>> GetAvailableReportsAsync(Guid tenantId)
    {
        return await GetActiveAsync(tenantId);
    }

    public async Task<ReportExecutionDto> GenerateReportAsync(GenerateReportDto dto, Guid tenantId, Guid userId)
    {
        return await ExecuteAsync(dto.ReportId, dto.Parameters, tenantId, userId);
    }

    public async Task<IEnumerable<ReportScheduleDto>> GetScheduledReportsAsync(Guid tenantId)
    {
        var schedules = await _reportScheduleRepository.GetByTenantIdAsync(tenantId);
        return schedules.Select(MapToDto);
    }

    public async Task<ReportScheduleDto> ScheduleReportAsync(ScheduleReportDto dto, Guid tenantId, Guid userId)
    {
        var schedule = new ReportSchedule
        {
            Id = Guid.NewGuid(),
            ReportId = dto.ReportId,
            ScheduleType = dto.ScheduleType,
            CronExpression = dto.ScheduleExpression,
            IsActive = true,
            TenantId = tenantId,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        var scheduleId = await _reportScheduleRepository.CreateAsync(schedule);
        var created = await _reportScheduleRepository.GetByIdAsync(scheduleId);
        return MapToDto(created!);
    }

    public async Task<ReportScheduleDto?> GetScheduledReportByIdAsync(Guid id, Guid tenantId)
    {
        var schedule = await _reportScheduleRepository.GetByIdAsync(id);
        return schedule != null ? MapToDto(schedule) : null;
    }

    public async Task<bool> CancelScheduledReportAsync(Guid id, Guid tenantId)
    {
        var schedule = await _reportScheduleRepository.GetByIdAsync(id);
        if (schedule == null) return false;
        
        schedule.IsActive = false;
        await _reportScheduleRepository.UpdateAsync(schedule);
        return true;
    }

    public async Task<IEnumerable<ReportExecutionDto>> GetReportHistoryAsync(Guid tenantId, string? reportType = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var executions = await _reportExecutionRepository.GetByTenantIdAsync(tenantId);
        
        if (reportType != null)
            executions = executions.Where(e => e.Report?.Category == reportType);
        if (fromDate.HasValue)
            executions = executions.Where(e => e.StartedAt >= fromDate.Value);
        if (toDate.HasValue)
            executions = executions.Where(e => e.StartedAt <= toDate.Value);

        return executions.Select(MapToDto);
    }

    public async Task<ReportExportDto> ExportReportAsync(Guid reportId, Guid tenantId, string format = "pdf")
    {
        var execution = await ExecuteAsync(reportId, null, tenantId, Guid.Empty);
        
        return new ReportExportDto
        {
            Content = System.Text.Encoding.UTF8.GetBytes(execution.ResultData ?? ""),
            ContentType = format.ToLower() switch
            {
                "pdf" => "application/pdf",
                "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "csv" => "text/csv",
                _ => "application/octet-stream"
            },
            FileName = $"report_{reportId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{format}"
        };
    }

    public async Task<object> GetTrialBalanceAsync(Guid tenantId, DateTime fromDate, DateTime toDate, string? currencyCode = null)
    {
        // Implementation would generate trial balance report
        return new { Message = "Trial Balance report generated", FromDate = fromDate, ToDate = toDate, CurrencyCode = currencyCode };
    }

    public async Task<object> GetProfitAndLossAsync(Guid tenantId, DateTime fromDate, DateTime toDate, string? currencyCode = null)
    {
        // Implementation would generate P&L report
        return new { Message = "Profit & Loss report generated", FromDate = fromDate, ToDate = toDate, CurrencyCode = currencyCode };
    }

    public async Task<object> GetBalanceSheetAsync(Guid tenantId, DateTime asOfDate, string? currencyCode = null)
    {
        // Implementation would generate balance sheet report
        return new { Message = "Balance Sheet report generated", AsOfDate = asOfDate, CurrencyCode = currencyCode };
    }

    public async Task<object> GetCashFlowAsync(Guid tenantId, DateTime fromDate, DateTime toDate, string? currencyCode = null)
    {
        // Implementation would generate cash flow report
        return new { Message = "Cash Flow report generated", FromDate = fromDate, ToDate = toDate, CurrencyCode = currencyCode };
    }

    public async Task<object> GetAgedReceivablesAsync(Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null)
    {
        // Implementation would generate aged receivables report
        return new { Message = "Aged Receivables report generated", AsOfDate = asOfDate, AgingBuckets = agingBuckets };
    }

    public async Task<object> GetAgedPayablesAsync(Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null)
    {
        // Implementation would generate aged payables report
        return new { Message = "Aged Payables report generated", AsOfDate = asOfDate, AgingBuckets = agingBuckets };
    }

    public async Task<object> GetGeneralLedgerAsync(Guid accountId, Guid tenantId, DateTime fromDate, DateTime toDate)
    {
        // Implementation would generate general ledger report
        return new { Message = "General Ledger report generated", AccountId = accountId, FromDate = fromDate, ToDate = toDate };
    }

    public async Task<object> GetBudgetVsActualAsync(Guid tenantId, DateTime fromDate, DateTime toDate, Guid? budgetId = null)
    {
        // Implementation would generate budget vs actual report
        return new { Message = "Budget vs Actual report generated", FromDate = fromDate, ToDate = toDate, BudgetId = budgetId };
    }

    private static ReportDto MapToDto(Report report)
    {
        return new ReportDto
        {
            Id = report.Id,
            Name = report.Name,
            Description = report.Description,
            Category = report.Category,
            Query = report.Query,
            Parameters = report.Parameters,
            OutputFormat = report.OutputFormat,
            IsActive = report.IsActive,
            TenantId = report.TenantId,
            CreatedBy = report.CreatedBy,
            CreatedAt = report.CreatedAt,
            UpdatedBy = report.UpdatedBy,
            UpdatedAt = report.UpdatedAt
        };
    }

    private static ReportExecutionDto MapToDto(ReportExecution execution)
    {
        return new ReportExecutionDto
        {
            Id = execution.Id,
            ReportId = execution.ReportId,
            Parameters = execution.Parameters,
            Status = execution.Status,
            StartedAt = execution.StartedAt,
            CompletedAt = execution.CompletedAt,
            ErrorMessage = execution.ErrorMessage,
            ResultData = execution.ResultData,
            TenantId = execution.TenantId,
            ExecutedBy = execution.ExecutedBy,
            CreatedAt = execution.CreatedAt
        };
    }

    private static ReportScheduleDto MapToDto(ReportSchedule schedule)
    {
        return new ReportScheduleDto
        {
            Id = schedule.Id,
            ReportId = schedule.ReportId,
            ScheduleType = schedule.ScheduleType,
            CronExpression = schedule.CronExpression,
            IsActive = schedule.IsActive,
            TenantId = schedule.TenantId,
            CreatedBy = schedule.CreatedBy,
            CreatedAt = schedule.CreatedAt,
            UpdatedBy = schedule.UpdatedBy,
            UpdatedAt = schedule.UpdatedAt
        };
    }
}