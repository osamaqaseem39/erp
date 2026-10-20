using ERP.Domain.HRM;

namespace ERP.Infrastructure.Interfaces.HRM;

public interface ITimeEntryRepository
{
    Task<TimeEntry?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<TimeEntry>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(TimeEntry entity);
    Task<bool> UpdateAsync(TimeEntry entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<TimeEntry>> GetByEmployeeAsync(Guid employeeId, Guid tenantId);
    Task<IEnumerable<TimeEntry>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
    Task<IEnumerable<TimeEntry>> GetByEmployeeAndDateRangeAsync(
        Guid employeeId, DateTime startDate, DateTime endDate, Guid tenantId);
}
