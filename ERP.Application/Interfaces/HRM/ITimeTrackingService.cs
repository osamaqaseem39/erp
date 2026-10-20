using ERP.Shared.DTOs.HRM;

namespace ERP.Application.Interfaces.HRM;

public interface ITimeTrackingService
{
    Task<TimeEntryDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<TimeEntryDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateTimeEntryDto dto, Guid tenantId);
    Task<bool> UpdateAsync(UpdateTimeEntryDto dto, Guid tenantId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<TimeEntryDto>> GetByEmployeeAsync(Guid employeeId, Guid tenantId);
    Task<IEnumerable<TimeEntryDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
    Task<decimal> GetTotalHoursAsync(Guid employeeId, DateTime startDate, DateTime endDate, Guid tenantId);
}
