using ERP.Domain.Manufacturing;

namespace ERP.Infrastructure.Repositories.Manufacturing;

public interface IProductionRunRepository
{
    Task<ProductionRun?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ProductionRun>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<ProductionRun>> GetByProductionOrderAsync(Guid productionOrderId, Guid tenantId);
    Task<IEnumerable<ProductionRun>> GetByStatusAsync(string status, Guid tenantId);
    Task<Guid> CreateAsync(ProductionRun productionRun);
    Task<bool> UpdateAsync(ProductionRun productionRun);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
