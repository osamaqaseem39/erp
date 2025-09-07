using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.SCM.Interfaces;

public interface IItemService
{
    Task<ItemDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ItemDto>> GetAllAsync(Guid tenantId, Guid companyId);
    Task<IEnumerable<ItemDto>> GetByCategoryAsync(Guid categoryId, Guid tenantId);
    Task<ItemDto> CreateAsync(CreateItemDto dto, Guid tenantId, string createdBy);
    Task<ItemDto> UpdateAsync(UpdateItemDto dto, Guid tenantId, string updatedBy);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> ExistsAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ItemDto>> GetLowStockItemsAsync(Guid tenantId, Guid companyId);
}