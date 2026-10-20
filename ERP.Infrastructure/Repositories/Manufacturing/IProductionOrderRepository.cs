using ERP.Domain.Manufacturing;

namespace ERP.Infrastructure.Repositories.Manufacturing;

public interface IProductionOrderRepository
{
    Task<ProductionOrder?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ProductionOrder>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<ProductionOrder>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<ProductionOrder>> GetByItemAsync(Guid itemId, Guid tenantId);
    Task<IEnumerable<ProductionOrder>> GetByWorkCenterAsync(Guid workCenterId, Guid tenantId);
    Task<ProductionOrder?> GetByNumberAsync(string productionOrderNumber, Guid tenantId);
    Task<Guid> CreateAsync(ProductionOrder productionOrder);
    Task<bool> UpdateAsync(ProductionOrder productionOrder);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
