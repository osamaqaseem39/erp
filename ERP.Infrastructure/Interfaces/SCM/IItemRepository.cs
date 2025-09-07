using ERP.Domain.SCM;

namespace ERP.Infrastructure.Interfaces.SCM;

public interface IItemRepository
{
    Task<Item?> GetByIdAsync(int id);
    Task<IEnumerable<Item>> GetAllAsync();
    Task<Item> AddAsync(Item entity);
    Task UpdateAsync(Item entity);
    Task DeleteAsync(int id);
}