using ERP.Shared.DTOs.HRM;

namespace ERP.Application.Interfaces;

public interface ILeaveService
{
    Task<LeaveRequestDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<LeaveRequestDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateLeaveRequestDto dto);
    Task<bool> UpdateAsync(UpdateLeaveRequestDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<LeaveRequestDto>> GetByEmployeeAsync(Guid employeeId);
    Task<IEnumerable<LeaveRequestDto>> GetByStatusAsync(string status);
    Task<bool> ApproveAsync(Guid id, Guid approvedBy);
    Task<bool> RejectAsync(Guid id, Guid rejectedBy, string reason);
}