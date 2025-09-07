using ERP.Domain.Reports;

namespace ERP.Infrastructure.Repositories.Reports;

public interface IReportExecutionRepository
{
    Task<ReportExecution?> GetByIdAsync(Guid id);
    Task<IEnumerable<ReportExecution>> GetAllAsync();
    Task<IEnumerable<ReportExecution>> GetByReportAsync(Guid reportId);
    Task<IEnumerable<ReportExecution>> GetByReportIdAsync(Guid reportId);
    Task<IEnumerable<ReportExecution>> GetByStatusAsync(string status);
    Task<IEnumerable<ReportExecution>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<ReportExecution>> GetByTenantIdAsync(Guid tenantId);
    Task<Guid> CreateAsync(ReportExecution reportExecution);
    Task<bool> UpdateAsync(ReportExecution reportExecution);
    Task<bool> DeleteAsync(Guid id);
}