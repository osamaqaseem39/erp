using ERP.Domain.HRM;

namespace ERP.Infrastructure.Repositories.HRM;

public interface ITimeEntryRepository
{
    Task<TimeEntry?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<TimeEntry>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<TimeEntry>> GetByEmployeeAsync(Guid employeeId, Guid tenantId);
    Task<IEnumerable<TimeEntry>> GetByProjectAsync(Guid projectId, Guid tenantId);
    Task<IEnumerable<TimeEntry>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
    Task<IEnumerable<TimeEntry>> GetByEmployeeAndDateRangeAsync(
        Guid employeeId, DateTime startDate, DateTime endDate, Guid tenantId);
    Task<Guid> CreateAsync(TimeEntry timeEntry);
    Task<bool> UpdateAsync(TimeEntry timeEntry);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
