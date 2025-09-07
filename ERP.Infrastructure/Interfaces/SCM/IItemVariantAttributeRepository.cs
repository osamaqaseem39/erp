using ERP.Domain.SCM;

namespace ERP.Infrastructure.Interfaces.SCM;

public interface IItemVariantAttributeRepository
{
    Task<ItemVariantAttribute?> GetByIdAsync(int id);
    Task<IEnumerable<ItemVariantAttribute>> GetAllAsync();
    Task<ItemVariantAttribute> AddAsync(ItemVariantAttribute entity);
    Task UpdateAsync(ItemVariantAttribute entity);
    Task DeleteAsync(int id);
}