using ERP.Domain.SCM;

namespace ERP.Infrastructure.Interfaces.SCM;

public interface IItemAttributeRepository
{
    Task<IEnumerable<ItemAttribute>> GetAllAsync(Guid tenantId, bool? isActive = null);
    Task<ItemAttribute?> GetByIdAsync(Guid id, Guid tenantId);
    Task<ItemAttribute> CreateAsync(ItemAttribute entity);
    Task<ItemAttribute> UpdateAsync(ItemAttribute entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}