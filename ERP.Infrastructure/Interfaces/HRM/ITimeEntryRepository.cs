using ERP.Domain.HRM;

namespace ERP.Infrastructure.Interfaces.HRM;

public interface ITimeEntryRepository
{
    Task<TimeEntry?> GetByIdAsync(Guid id);
    Task<IEnumerable<TimeEntry>> GetAllAsync();
    Task<Guid> CreateAsync(TimeEntry entity);
    Task<bool> UpdateAsync(TimeEntry entity);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<TimeEntry>> GetByEmployeeAsync(Guid employeeId);
    Task<IEnumerable<TimeEntry>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<TimeEntry>> GetByEmployeeAndDateRangeAsync(Guid employeeId, DateTime startDate, DateTime endDate);
}