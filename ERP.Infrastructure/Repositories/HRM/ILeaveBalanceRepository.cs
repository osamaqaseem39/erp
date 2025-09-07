using ERP.Domain.HRM;

namespace ERP.Infrastructure.Repositories.HRM;

public interface ILeaveBalanceRepository
{
    Task<LeaveBalance?> GetByIdAsync(Guid id);
    Task<IEnumerable<LeaveBalance>> GetAllAsync();
    Task<IEnumerable<LeaveBalance>> GetByEmployeeAsync(Guid employeeId);
    Task<IEnumerable<LeaveBalance>> GetByLeaveTypeAsync(Guid leaveTypeId);
    Task<LeaveBalance?> GetByEmployeeAndLeaveTypeAsync(Guid employeeId, Guid leaveTypeId);
    Task<Guid> CreateAsync(LeaveBalance leaveBalance);
    Task<bool> UpdateAsync(LeaveBalance leaveBalance);
    Task<bool> DeleteAsync(Guid id);
}