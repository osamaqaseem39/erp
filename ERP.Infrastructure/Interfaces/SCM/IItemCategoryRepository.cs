using ERP.Domain.SCM;

namespace ERP.Infrastructure.Interfaces.SCM;

public interface IItemCategoryRepository
{
    Task<ItemCategory?> GetByIdAsync(int id);
    Task<IEnumerable<ItemCategory>> GetAllAsync();
    Task<ItemCategory> AddAsync(ItemCategory entity);
    Task UpdateAsync(ItemCategory entity);
    Task DeleteAsync(int id);
}