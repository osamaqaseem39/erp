using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Interfaces;

public interface IItemService
{
    Task<ItemDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ItemDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateItemDto dto);
    Task<bool> UpdateAsync(UpdateItemDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<ItemDto>> GetByCategoryAsync(Guid categoryId);
    Task<IEnumerable<ItemDto>> GetByTypeAsync(string itemType);
    Task<IEnumerable<ItemDto>> SearchAsync(string searchTerm);
    Task<decimal> GetStockLevelAsync(Guid itemId, Guid warehouseId);
}