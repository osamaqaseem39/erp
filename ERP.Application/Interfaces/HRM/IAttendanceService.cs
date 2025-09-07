using ERP.Shared.DTOs.HRM;

namespace ERP.Application.Interfaces.HRM;

public interface IAttendanceService
{
    Task<object> CheckInAsync(CheckInDto dto, Guid tenantId, Guid userId);
    Task<object> CheckOutAsync(CheckOutDto dto, Guid tenantId, Guid userId);
    Task<IEnumerable<object>> GetEmployeeAttendanceAsync(Guid employeeId, Guid tenantId, DateTime fromDate, DateTime toDate);
    Task<object> GetAttendanceSummaryAsync(Guid employeeId, Guid tenantId, DateTime fromDate, DateTime toDate);
    Task<object> AdjustAttendanceAsync(AttendanceAdjustmentDto dto, Guid tenantId, Guid userId);
}