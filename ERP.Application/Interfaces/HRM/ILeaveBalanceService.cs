using ERP.Shared.DTOs.HRM;

namespace ERP.Application.Interfaces.HRM;

public interface ILeaveBalanceService
{
    Task<IEnumerable<LeaveBalanceDto>> GetAllAsync(Guid tenantId, Guid? employeeId = null, Guid? leaveTypeId = null);
}
