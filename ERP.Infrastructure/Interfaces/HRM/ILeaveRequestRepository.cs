using ERP.Domain.HRM;

namespace ERP.Infrastructure.Interfaces.HRM;

public interface ILeaveRequestRepository
{
    Task<LeaveRequest?> GetByIdAsync(Guid id);
    Task<IEnumerable<LeaveRequest>> GetAllAsync();
    Task<Guid> CreateAsync(LeaveRequest leaveRequest);
    Task<bool> UpdateAsync(LeaveRequest leaveRequest);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(Guid employeeId);
    Task<IEnumerable<LeaveRequest>> GetByStatusAsync(string status);
    Task<IEnumerable<LeaveRequest>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}