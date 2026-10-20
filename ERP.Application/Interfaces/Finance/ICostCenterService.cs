using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Interfaces.Finance;

public interface ICostCenterService
{
    Task<Guid> CreateAsync(CreateCostCenterDto dto, Guid tenantId, Guid userId);
    Task<Guid> UpdateAsync(UpdateCostCenterDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> SoftDeactivateAsync(Guid id, Guid tenantId);
    Task<CostCenterDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<CostCenterDto>> GetAllAsync(Guid tenantId, bool? isActive = null);
}
