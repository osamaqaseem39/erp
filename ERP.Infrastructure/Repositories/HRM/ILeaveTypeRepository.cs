using ERP.Domain.HRM;

namespace ERP.Infrastructure.Repositories.HRM;

public interface ILeaveTypeRepository
{
    Task<LeaveType?> GetByIdAsync(Guid id);
    Task<IEnumerable<LeaveType>> GetAllAsync();
    Task<IEnumerable<LeaveType>> GetActiveAsync();
    Task<Guid> CreateAsync(LeaveType leaveType);
    Task<bool> UpdateAsync(LeaveType leaveType);
    Task<bool> DeleteAsync(Guid id);
}