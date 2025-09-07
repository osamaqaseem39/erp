using ERP.Domain.SCM;

namespace ERP.Infrastructure.Interfaces.SCM;

public interface IInventoryValuationMethodRepository
{
    Task<IEnumerable<InventoryValuationMethod>> GetAllAsync(Guid tenantId, bool? isActive = null);
    Task<InventoryValuationMethod?> GetByIdAsync(Guid id, Guid tenantId);
    Task<InventoryValuationMethod> CreateAsync(InventoryValuationMethod entity);
    Task<InventoryValuationMethod> UpdateAsync(InventoryValuationMethod entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}