using ERP.Shared.DTOs.HRM;

namespace ERP.Application.Interfaces.HRM;

public interface ILeaveRequestService
{
    Task<LeaveRequestDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<LeaveRequestDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateLeaveRequestDto dto);
    Task<bool> UpdateAsync(UpdateLeaveRequestDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<LeaveRequestDto>> GetByEmployeeAsync(Guid employeeId);
    Task<IEnumerable<LeaveRequestDto>> GetByStatusAsync(string status);
    Task<bool> ApproveAsync(Guid id);
    Task<bool> RejectAsync(Guid id);
    Task<bool> CancelAsync(Guid id, LeaveCancellationDto dto, Guid tenantId, Guid userId);
}