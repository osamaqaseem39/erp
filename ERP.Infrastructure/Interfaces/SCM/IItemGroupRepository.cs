using ERP.Domain.SCM;

namespace ERP.Infrastructure.Interfaces.SCM;

public interface IItemGroupRepository
{
    Task<IEnumerable<ItemGroup>> GetAllAsync(Guid tenantId, bool? isActive = null);
    Task<ItemGroup?> GetByIdAsync(Guid id, Guid tenantId);
    Task<ItemGroup> CreateAsync(ItemGroup entity);
    Task<ItemGroup> UpdateAsync(ItemGroup entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}