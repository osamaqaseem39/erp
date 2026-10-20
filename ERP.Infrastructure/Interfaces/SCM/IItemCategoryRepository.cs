using ERP.Domain.SCM;

namespace ERP.Infrastructure.Interfaces.SCM;

public interface IItemCategoryRepository
{
    Task<ItemCategory?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ItemCategory>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<ItemCategory>> GetActiveAsync(Guid tenantId);
    Task<IEnumerable<ItemCategory>> GetByParentAsync(Guid tenantId, Guid? parentId);
    Task<bool> CodeExistsAsync(Guid tenantId, string code, Guid? excludeId = null);
    Task<ItemCategory> AddAsync(ItemCategory entity);
    Task UpdateAsync(ItemCategory entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
