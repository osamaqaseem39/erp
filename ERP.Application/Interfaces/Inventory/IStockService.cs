using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Interfaces.Inventory;

public interface IStockService
{
    Task<StockBalanceDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<StockBalanceDto>> GetAllAsync();
    Task<Guid> CreateAsync(StockBalanceDto dto);
    Task<bool> UpdateAsync(StockBalanceDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<StockBalanceDto>> GetByItemAsync(Guid itemId);
    Task<IEnumerable<StockBalanceDto>> GetByWarehouseAsync(Guid warehouseId);
    Task<IEnumerable<StockBalanceDto>> GetByItemAndWarehouseAsync(Guid itemId, Guid warehouseId);
    Task<decimal> GetStockLevelAsync(Guid itemId, Guid warehouseId);
    Task<bool> AdjustStockAsync(StockAdjustmentDto dto);
}