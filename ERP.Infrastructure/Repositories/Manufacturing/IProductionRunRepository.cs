using ERP.Domain.Manufacturing;

namespace ERP.Infrastructure.Repositories.Manufacturing;

public interface IProductionRunRepository
{
    Task<ProductionRun?> GetByIdAsync(Guid id);
    Task<IEnumerable<ProductionRun>> GetAllAsync();
    Task<IEnumerable<ProductionRun>> GetByProductionOrderAsync(Guid productionOrderId);
    Task<IEnumerable<ProductionRun>> GetByStatusAsync(string status);
    Task<Guid> CreateAsync(ProductionRun productionRun);
    Task<bool> UpdateAsync(ProductionRun productionRun);
    Task<bool> DeleteAsync(Guid id);
}