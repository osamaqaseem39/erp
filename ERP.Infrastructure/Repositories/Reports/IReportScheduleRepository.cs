using ERP.Domain.Reports;

namespace ERP.Infrastructure.Repositories.Reports;

public interface IReportScheduleRepository
{
    Task<ReportSchedule?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ReportSchedule>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<ReportSchedule>> GetByReportAsync(Guid reportId, Guid tenantId);
    Task<IEnumerable<ReportSchedule>> GetActiveAsync(Guid tenantId);
    /// <summary>Background worker: due schedules across tenants (intentional).</summary>
    Task<IEnumerable<ReportSchedule>> GetScheduledForExecutionAsync();
    Task<IEnumerable<ReportSchedule>> GetByTenantIdAsync(Guid tenantId);
    Task<Guid> CreateAsync(ReportSchedule reportSchedule);
    Task<bool> UpdateAsync(ReportSchedule reportSchedule);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
