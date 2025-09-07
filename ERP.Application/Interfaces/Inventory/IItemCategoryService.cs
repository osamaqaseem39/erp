using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Interfaces.Inventory;

public interface IItemCategoryService
{
    Task<ItemCategoryDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ItemCategoryDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateItemCategoryDto dto);
    Task<bool> UpdateAsync(UpdateItemCategoryDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<ItemCategoryDto>> GetActiveAsync();
    Task<IEnumerable<ItemCategoryDto>> GetByParentAsync(Guid? parentId);
}