using ERP.Shared.DTOs.Manufacturing;

namespace ERP.Application.Interfaces.Manufacturing;

public interface IWorkCenterService
{
    Task<WorkCenterDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<WorkCenterDto>> GetAllAsync(Guid tenantId, bool? isActive = null);
    Task<IEnumerable<WorkCenterDto>> GetActiveAsync(Guid tenantId);
    Task<WorkCenterDto> CreateAsync(CreateWorkCenterDto dto, Guid tenantId, Guid userId);
    Task<WorkCenterDto> UpdateAsync(UpdateWorkCenterDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}