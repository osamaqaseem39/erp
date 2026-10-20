using ERP.Shared.DTOs.HRM;

namespace ERP.Application.Interfaces.HRM;

public interface ILeaveTypeService
{
    Task<LeaveTypeDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<LeaveTypeDto>> GetAllAsync(Guid tenantId);
    Task<LeaveTypeDto> CreateAsync(CreateLeaveTypeDto dto, Guid tenantId, Guid userId);
    Task<LeaveTypeDto> UpdateAsync(UpdateLeaveTypeDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
