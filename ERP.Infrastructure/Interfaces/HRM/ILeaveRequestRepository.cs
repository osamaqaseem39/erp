using ERP.Domain.HRM;

namespace ERP.Infrastructure.Interfaces.HRM;

public interface ILeaveRequestRepository
{
    Task<LeaveRequest?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<LeaveRequest>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(LeaveRequest leaveRequest);
    Task<bool> UpdateAsync(LeaveRequest leaveRequest);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> ExistsAsync(Guid id, Guid tenantId);
    Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(Guid employeeId, Guid tenantId);
    Task<IEnumerable<LeaveRequest>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<LeaveRequest>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
}
