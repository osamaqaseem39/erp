using ERP.Shared.DTOs.HRM;

namespace ERP.Application.Interfaces.HRM;

public interface ILeaveService
{
    Task<LeaveRequestDto> AddLeaveRequestAsync(CreateLeaveRequestDto dto);
    Task<LeaveRequestDto> AddLeaveRequestAsync(CreateLeaveRequestDto dto, Guid tenantId, Guid userId);
    Task<LeaveRequestDto> UpdateLeaveRequestAsync(UpdateLeaveRequestDto dto, Guid tenantId);
    Task<bool> DeleteLeaveRequestAsync(Guid leaveRequestId, Guid tenantId);
    Task<LeaveRequestDto?> GetLeaveRequestByIdAsync(Guid leaveRequestId, Guid tenantId);
    Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByEmployeeAsync(Guid employeeId, Guid tenantId);
    Task<IEnumerable<LeaveRequestDto>> GetPendingLeaveRequestsAsync(Guid tenantId);
    Task<bool> HasLeaveOverlapAsync(Guid employeeId, DateTime startDate, DateTime endDate, Guid tenantId, Guid? excludeRequestId = null);
    Task<LeaveRequestDto> ApproveLeaveAsync(LeaveApprovalDto dto, Guid tenantId, Guid approverId);
    Task<LeaveRequestDto> RejectLeaveAsync(RejectLeaveRequestDto dto, Guid tenantId, Guid rejectedBy);
    Task<LeaveRequestDto> CancelLeaveAsync(Guid leaveRequestId, string? reason, Guid tenantId, Guid userId);
    Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByStatusAsync(string status, Guid tenantId);
}
