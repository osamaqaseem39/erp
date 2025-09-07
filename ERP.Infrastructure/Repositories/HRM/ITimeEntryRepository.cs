using ERP.Domain.HRM;

namespace ERP.Infrastructure.Repositories.HRM;

public interface ITimeEntryRepository
{
    Task<TimeEntry?> GetByIdAsync(Guid id);
    Task<IEnumerable<TimeEntry>> GetAllAsync();
    Task<IEnumerable<TimeEntry>> GetByEmployeeAsync(Guid employeeId);
    Task<IEnumerable<TimeEntry>> GetByProjectAsync(Guid projectId);
    Task<IEnumerable<TimeEntry>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Guid> CreateAsync(TimeEntry timeEntry);
    Task<bool> UpdateAsync(TimeEntry timeEntry);
    Task<bool> DeleteAsync(Guid id);
}