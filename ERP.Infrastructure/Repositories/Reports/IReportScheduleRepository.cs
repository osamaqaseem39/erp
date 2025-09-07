using ERP.Domain.Reports;

namespace ERP.Infrastructure.Repositories.Reports;

public interface IReportScheduleRepository
{
    Task<ReportSchedule?> GetByIdAsync(Guid id);
    Task<IEnumerable<ReportSchedule>> GetAllAsync();
    Task<IEnumerable<ReportSchedule>> GetByReportAsync(Guid reportId);
    Task<IEnumerable<ReportSchedule>> GetActiveAsync();
    Task<IEnumerable<ReportSchedule>> GetScheduledForExecutionAsync();
    Task<IEnumerable<ReportSchedule>> GetByTenantIdAsync(Guid tenantId);
    Task<Guid> CreateAsync(ReportSchedule reportSchedule);
    Task<bool> UpdateAsync(ReportSchedule reportSchedule);
    Task<bool> DeleteAsync(Guid id);
}