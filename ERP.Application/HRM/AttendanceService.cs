using ERP.Application.Interfaces.HRM;
using ERP.Shared.DTOs.HRM;
using System;
using System.Linq;

namespace ERP.Application.HRM;

/// <summary>DI stub for IAttendanceService. Replace with real business logic incrementally.</summary>
public class AttendanceService : IAttendanceService
{
    public Task<object> CheckInAsync(CheckInDto dto, Guid tenantId, Guid userId)
    {
        return Task.FromResult<object>(new { });
    }

    public Task<object> CheckOutAsync(CheckOutDto dto, Guid tenantId, Guid userId)
    {
        return Task.FromResult<object>(new { });
    }

    public Task<IEnumerable<object>> GetEmployeeAttendanceAsync(Guid employeeId, Guid tenantId, DateTime fromDate, DateTime toDate)
    {
        return Task.FromResult(Enumerable.Empty<object>());
    }

    public Task<object> GetAttendanceSummaryAsync(Guid employeeId, Guid tenantId, DateTime fromDate, DateTime toDate)
    {
        return Task.FromResult<object>(new { });
    }

    public Task<object> AdjustAttendanceAsync(AttendanceAdjustmentDto dto, Guid tenantId, Guid userId)
    {
        return Task.FromResult<object>(new { });
    }

}
