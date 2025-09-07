using ERP.Domain.SCM;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.SCM.Interfaces;

public interface IStockBalanceRepository
{
    Task<StockBalance?> GetByItemAndWarehouseAsync(Guid itemId, Guid warehouseId, Guid tenantId);
    Task<IEnumerable<StockBalance>> GetAllAsync(Guid tenantId, Guid companyId, Guid? warehouseId = null);
    Task<StockBalance> UpdateAsync(StockBalance stockBalance);
    Task<StockBalance> AddAsync(StockBalance stockBalance);
    Task<bool> ExistsAsync(Guid itemId, Guid warehouseId, Guid tenantId);
    Task<decimal> GetAvailableQuantityAsync(Guid itemId, Guid warehouseId, Guid tenantId);
    Task<IEnumerable<StockBalance>> GetLowStockItemsAsync(Guid tenantId, Guid companyId);
}