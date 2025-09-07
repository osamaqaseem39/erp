using ERP.Domain.SCM;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.SCM.Interfaces;

public interface IItemRepository
{
    Task<Item?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Item>> GetAllAsync(Guid tenantId, Guid companyId);
    Task<IEnumerable<Item>> GetByCategoryAsync(Guid categoryId, Guid tenantId);
    Task<Item> AddAsync(Item item);
    Task<Item> UpdateAsync(Item item);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> ExistsAsync(Guid id, Guid tenantId);
    Task<bool> CodeExistsAsync(string code, Guid tenantId, Guid companyId, Guid? excludeId = null);
    Task<IEnumerable<Item>> GetLowStockItemsAsync(Guid tenantId, Guid companyId);
}