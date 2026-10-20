using ERP.Domain.SCM;

namespace ERP.Infrastructure.Repositories.SCM;

public interface IStockAdjustmentRepository
{
    Task<StockAdjustment?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<StockAdjustment>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<StockAdjustment>> GetByWarehouseAsync(Guid warehouseId, Guid tenantId);
    Task<IEnumerable<StockAdjustment>> GetByItemAsync(Guid itemId, Guid tenantId);
    Task<IEnumerable<StockAdjustment>> GetByDateRangeAsync(Guid tenantId, DateTime start, DateTime end);
    Task<Guid> CreateAsync(StockAdjustment entity);
    Task<bool> UpdateAsync(StockAdjustment entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
