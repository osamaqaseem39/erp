using ERP.Shared.DTOs.HRM;

namespace ERP.Application.Interfaces.HRM;

public interface ITimeTrackingService
{
    Task<TimeEntryDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<TimeEntryDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateTimeEntryDto dto);
    Task<bool> UpdateAsync(UpdateTimeEntryDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<TimeEntryDto>> GetByEmployeeAsync(Guid employeeId);
    Task<IEnumerable<TimeEntryDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalHoursAsync(Guid employeeId, DateTime startDate, DateTime endDate);
}