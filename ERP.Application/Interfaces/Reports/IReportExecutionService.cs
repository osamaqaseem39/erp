using ERP.Shared.DTOs.Reports;

namespace ERP.Application.Interfaces.Reports;

public interface IReportExecutionService
{
    Task<ReportExecutionDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ReportExecutionDto>> GetAllAsync();
    Task<IEnumerable<ReportExecutionDto>> GetByReportAsync(Guid reportId);
    Task<IEnumerable<ReportExecutionDto>> GetByStatusAsync(string status);
    Task<IEnumerable<ReportExecutionDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Guid> CreateAsync(CreateReportExecutionDto dto);
    Task<bool> UpdateAsync(UpdateReportExecutionDto dto);
    Task<bool> DeleteAsync(Guid id);
}