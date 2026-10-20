using ERP.Domain.HRM;

namespace ERP.Infrastructure.Repositories.HRM;

public interface ILeaveRequestRepository
{
    Task<LeaveRequest?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<LeaveRequest>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(Guid employeeId, Guid tenantId);
    Task<IEnumerable<LeaveRequest>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<LeaveRequest>> GetByLeaveTypeAsync(Guid leaveTypeId, Guid tenantId);
    Task<IEnumerable<LeaveRequest>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
    Task<Guid> CreateAsync(LeaveRequest leaveRequest);
    Task<bool> UpdateAsync(LeaveRequest leaveRequest);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
