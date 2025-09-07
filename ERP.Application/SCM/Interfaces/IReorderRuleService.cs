using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.SCM.Interfaces;

public interface IReorderRuleService
{
    Task<IEnumerable<ReorderRuleDto>> GetByItemAsync(Guid? itemId, Guid? warehouseId, Guid tenantId);
    Task<ReorderRuleDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<ReorderRuleDto> CreateAsync(ReorderRuleDto dto, Guid tenantId, string createdBy);
    Task<ReorderRuleDto> UpdateAsync(ReorderRuleDto dto, Guid tenantId, string updatedBy);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}