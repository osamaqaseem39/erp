using ERP.Domain.Manufacturing;

namespace ERP.Infrastructure.Repositories.Manufacturing;

public interface IProductionOrderLineRepository
{
    Task<ProductionOrderLine?> GetByIdAsync(Guid id);
    Task<IEnumerable<ProductionOrderLine>> GetAllAsync();
    Task<IEnumerable<ProductionOrderLine>> GetByProductionOrderAsync(Guid productionOrderId);
    Task<IEnumerable<ProductionOrderLine>> GetByItemAsync(Guid itemId);
    Task<Guid> CreateAsync(ProductionOrderLine productionOrderLine);
    Task<bool> UpdateAsync(ProductionOrderLine productionOrderLine);
    Task<bool> DeleteAsync(Guid id);
}