using ERP.Domain.SCM;

namespace ERP.Infrastructure.Interfaces.SCM;

public interface IReorderRuleRepository
{
    Task<IEnumerable<ReorderRule>> GetByItemAsync(Guid? itemId, Guid? warehouseId, Guid tenantId);
    Task<ReorderRule?> GetByIdAsync(Guid id, Guid tenantId);
    Task<ReorderRule> CreateAsync(ReorderRule entity);
    Task<ReorderRule> UpdateAsync(ReorderRule entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}