using ERP.Domain.SCM;

namespace ERP.Infrastructure.Interfaces.SCM;

public interface IStockBalanceRepository
{
    Task<StockBalance?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<StockBalance>> GetAllAsync(Guid tenantId);
    Task<StockBalance> CreateAsync(StockBalance entity);
    Task<StockBalance> UpdateAsync(StockBalance entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    /// <summary>Warehouse-level default bucket (WarehouseLocationId null).</summary>
    Task<StockBalance?> GetByItemAndWarehouseAsync(Guid itemId, Guid warehouseId, Guid tenantId);
    Task<StockBalance?> GetByItemWarehouseLocationAsync(Guid itemId, Guid warehouseId, Guid? warehouseLocationId, Guid tenantId);
    Task<IEnumerable<StockBalance>> GetByItemAsync(Guid itemId, Guid tenantId);
    Task<IEnumerable<StockBalance>> GetByWarehouseAsync(Guid warehouseId, Guid tenantId);
    Task<IEnumerable<StockBalance>> GetByItemAndWarehouseAllLocationsAsync(Guid itemId, Guid warehouseId, Guid tenantId);
}