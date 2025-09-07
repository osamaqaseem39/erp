using ERP.Domain.SCM;

namespace ERP.Infrastructure.Interfaces.Inventory;

public interface IItemRepository
{
    Task<Item?> GetByIdAsync(Guid id);
    Task<IEnumerable<Item>> GetAllAsync();
    Task<Item> CreateAsync(Item item);
    Task<Item> UpdateAsync(Item item);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Item?> GetByCodeAsync(string code);
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);
    Task<Item?> GetBySkuAsync(string sku);
    Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeId = null);
}