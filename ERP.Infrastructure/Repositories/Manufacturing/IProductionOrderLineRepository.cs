using ERP.Domain.Manufacturing;

namespace ERP.Infrastructure.Repositories.Manufacturing;

public interface IProductionOrderLineRepository
{
    Task<ProductionOrderLine?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ProductionOrderLine>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<ProductionOrderLine>> GetByProductionOrderAsync(Guid productionOrderId, Guid tenantId);
    Task<IEnumerable<ProductionOrderLine>> GetByItemAsync(Guid itemId, Guid tenantId);
    Task<Guid> CreateAsync(ProductionOrderLine productionOrderLine);
    Task<bool> UpdateAsync(ProductionOrderLine productionOrderLine);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
