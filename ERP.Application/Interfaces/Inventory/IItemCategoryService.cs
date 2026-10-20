using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Interfaces.Inventory;

public interface IItemCategoryService
{
    Task<ItemCategoryDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ItemCategoryDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateItemCategoryDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateItemCategoryDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ItemCategoryDto>> GetActiveAsync(Guid tenantId);
    Task<IEnumerable<ItemCategoryDto>> GetByParentAsync(Guid? parentId, Guid tenantId);
}
