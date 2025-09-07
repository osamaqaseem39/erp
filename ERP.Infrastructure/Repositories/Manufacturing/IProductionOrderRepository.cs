using ERP.Domain.Manufacturing;

namespace ERP.Infrastructure.Repositories.Manufacturing;

public interface IProductionOrderRepository
{
    Task<ProductionOrder?> GetByIdAsync(Guid id);
    Task<IEnumerable<ProductionOrder>> GetAllAsync();
    Task<IEnumerable<ProductionOrder>> GetByStatusAsync(string status);
    Task<IEnumerable<ProductionOrder>> GetByItemAsync(Guid itemId);
    Task<IEnumerable<ProductionOrder>> GetByWorkCenterAsync(Guid workCenterId);
    Task<ProductionOrder?> GetByNumberAsync(string productionOrderNumber);
    Task<Guid> CreateAsync(ProductionOrder productionOrder);
    Task<bool> UpdateAsync(ProductionOrder productionOrder);
    Task<bool> DeleteAsync(Guid id);
}