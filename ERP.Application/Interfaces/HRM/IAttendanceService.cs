using ERP.Shared.DTOs.HRM;

namespace ERP.Application.Interfaces.HRM;

public interface IAttendanceService
{
    Task<AttendanceRecordDto> CheckInAsync(CheckInDto dto, Guid tenantId, Guid userId);
    Task<AttendanceRecordDto> CheckOutAsync(CheckOutDto dto, Guid tenantId, Guid userId);
    Task<IEnumerable<AttendanceRecordDto>> GetEmployeeAttendanceAsync(Guid employeeId, Guid tenantId, DateTime fromDate, DateTime toDate);
    Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(Guid employeeId, Guid tenantId, DateTime fromDate, DateTime toDate);
    Task<AttendanceRecordDto> AdjustAttendanceAsync(AttendanceAdjustmentDto dto, Guid tenantId, Guid userId);
}
