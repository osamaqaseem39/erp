using ERP.Shared.DTOs.Inventory;

namespace ERP.Api.Services;

public interface IStockApiService
{
    Task<StockBalanceDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<StockBalanceDto>> GetAllAsync();
    Task<IEnumerable<StockBalanceDto>> GetByItemAsync(Guid itemId);
    Task<IEnumerable<StockBalanceDto>> GetByWarehouseAsync(Guid warehouseId);
    Task<decimal> GetStockLevelAsync(Guid itemId, Guid warehouseId);
    Task<bool> AdjustStockAsync(StockAdjustmentDto dto);
    Task<bool> TransferStockAsync(StockTransferDto dto);
    Task<IEnumerable<StockLedgerDto>> GetStockLedgerAsync(Guid itemId, Guid warehouseId);
}