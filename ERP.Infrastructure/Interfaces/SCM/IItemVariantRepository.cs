using ERP.Domain.SCM;

namespace ERP.Infrastructure.Interfaces.SCM;

public interface IItemVariantRepository
{
    Task<IEnumerable<ItemVariant>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<ItemVariant>> GetByItemAsync(Guid itemId, Guid tenantId);
    Task<ItemVariant?> GetByIdAsync(Guid id, Guid tenantId);
    Task<ItemVariant> CreateAsync(ItemVariant entity);
    Task<ItemVariant> UpdateAsync(ItemVariant entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}