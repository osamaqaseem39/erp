using ERP.Domain.Reports;

namespace ERP.Infrastructure.Repositories.Reports;

public interface IReportExecutionRepository
{
    Task<ReportExecution?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ReportExecution>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<ReportExecution>> GetByReportAsync(Guid reportId, Guid tenantId);
    Task<IEnumerable<ReportExecution>> GetByReportIdAsync(Guid reportId, Guid tenantId);
    Task<IEnumerable<ReportExecution>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<ReportExecution>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
    Task<IEnumerable<ReportExecution>> GetByTenantIdAsync(Guid tenantId);
    Task<Guid> CreateAsync(ReportExecution reportExecution);
    Task<bool> UpdateAsync(ReportExecution reportExecution);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
