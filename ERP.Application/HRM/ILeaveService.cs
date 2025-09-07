using ERP.Shared.DTOs.HRM;

namespace ERP.Application.HRM;

public interface ILeaveService
{
    Task<LeaveRequestDto> AddLeaveRequestAsync(CreateLeaveRequestDto dto);
    Task<LeaveRequestDto> UpdateLeaveRequestAsync(UpdateLeaveRequestDto dto);
    Task<LeaveRequestDto> ApproveLeaveAsync(LeaveApprovalDto dto, Guid approverId);
    Task DeleteLeaveRequestAsync(Guid leaveRequestId);
    Task<LeaveRequestDto> GetLeaveRequestByIdAsync(Guid leaveRequestId);
    Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByEmployeeAsync(Guid employeeId);
    Task<IEnumerable<LeaveRequestDto>> GetPendingLeaveRequestsAsync();
    Task<bool> HasLeaveOverlapAsync(Guid employeeId, DateTime startDate, DateTime endDate, Guid? excludeRequestId = null);
}