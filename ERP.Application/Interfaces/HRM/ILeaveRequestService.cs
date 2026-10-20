using ERP.Shared.DTOs.HRM;

namespace ERP.Application.Interfaces.HRM;

public interface ILeaveRequestService
{
    Task<LeaveRequestDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<LeaveRequestDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateLeaveRequestDto dto);
    Task<bool> UpdateAsync(UpdateLeaveRequestDto dto, Guid tenantId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<LeaveRequestDto>> GetByEmployeeAsync(Guid employeeId, Guid tenantId);
    Task<IEnumerable<LeaveRequestDto>> GetByStatusAsync(string status, Guid tenantId);
    Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> RejectAsync(Guid id, Guid tenantId, Guid userId, string? reason = null);
    Task<bool> CancelAsync(Guid id, LeaveCancellationDto dto, Guid tenantId, Guid userId);
}
