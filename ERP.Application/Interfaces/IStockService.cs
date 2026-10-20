using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Interfaces;

public interface IStockService
{
    Task<StockBalanceDto?> GetByIdAsync(Guid id);
    Task<StockBalanceDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<StockBalanceDto>> GetAllAsync();
    Task<IEnumerable<StockBalanceDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(StockBalanceDto dto);
    Task<Guid> CreateAsync(StockBalanceDto dto, Guid tenantId);
    Task<bool> UpdateAsync(StockBalanceDto dto);
    Task<bool> UpdateAsync(StockBalanceDto dto, Guid tenantId);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<StockBalanceDto?> GetByItemAndWarehouseAsync(Guid itemId, Guid warehouseId);
    Task<StockBalanceDto?> GetByItemAndWarehouseAsync(Guid itemId, Guid warehouseId, Guid tenantId);
    Task<StockBalanceDto?> GetByItemWarehouseLocationAsync(Guid itemId, Guid warehouseId, Guid? warehouseLocationId, Guid tenantId);
    Task<IEnumerable<StockBalanceDto>> GetByItemAsync(Guid itemId);
    Task<IEnumerable<StockBalanceDto>> GetByItemAsync(Guid itemId, Guid tenantId);
    Task<IEnumerable<StockBalanceDto>> GetByWarehouseAsync(Guid warehouseId);
    Task<IEnumerable<StockBalanceDto>> GetByWarehouseAsync(Guid warehouseId, Guid tenantId);
    Task<decimal> GetStockLevelAsync(Guid itemId, Guid warehouseId);
    Task<decimal> GetStockLevelAsync(Guid itemId, Guid warehouseId, Guid tenantId);
    Task<bool> AdjustStockAsync(StockAdjustmentDto dto);
    Task<bool> AdjustStockAsync(StockAdjustmentDto dto, Guid tenantId);
    Task<bool> TransferStockAsync(StockTransferDto dto);
    /// <summary>Increase on-hand quantity (e.g. GRN post). Uses warehouse default bucket unless location provided.</summary>
    Task<bool> ReceiveStockAsync(Guid tenantId, Guid warehouseId, IEnumerable<(Guid ItemId, decimal Quantity)> lines, string? userId = null);
    /// <summary>Receipt with explicit unit costs for WAC.</summary>
    Task<bool> ReceiveStockAsync(Guid tenantId, Guid warehouseId, IEnumerable<(Guid ItemId, decimal Quantity, decimal UnitCost)> lines, string? userId = null);
    /// <summary>Location-aware receipt (putaway). When inventory.requireBin is true, each line needs a location.</summary>
    Task<bool> ReceiveStockAsync(Guid tenantId, Guid warehouseId, IEnumerable<(Guid ItemId, decimal Quantity, Guid? WarehouseLocationId)> lines, string? userId = null);
    /// <summary>Decrease on-hand quantity (e.g. sales ship). Fails if any line lacks enough stock.</summary>
    Task<bool> IssueStockAsync(Guid tenantId, Guid warehouseId, IEnumerable<(Guid ItemId, decimal Quantity)> lines, string? userId = null);
    /// <summary>Location-aware issue (pick). When inventory.requireBin is true, each line needs a location.</summary>
    Task<bool> IssueStockAsync(Guid tenantId, Guid warehouseId, IEnumerable<(Guid ItemId, decimal Quantity, Guid? WarehouseLocationId)> lines, string? userId = null);
    /// <summary>Issue from any warehouse that has stock (tenant-scoped). Returns false if short.</summary>
    Task<bool> IssueStockAnyWarehouseAsync(Guid tenantId, IEnumerable<(Guid ItemId, decimal Quantity)> lines, string? userId = null);
    /// <summary>Soft-allocate qty on warehouse default bucket. Fails if Available &lt; qty.</summary>
    Task<bool> ReserveAsync(Guid tenantId, Guid warehouseId, Guid itemId, decimal quantity, string? userId = null);
    /// <summary>Release soft allocation. Clamps ReservedQuantity at 0.</summary>
    Task<bool> UnreserveAsync(Guid tenantId, Guid warehouseId, Guid itemId, decimal quantity, string? userId = null);
    Task<IEnumerable<StockLedgerDto>> GetStockLedgerAsync(Guid itemId, Guid tenantId, Guid? warehouseId = null, DateTime? toDate = null);
    Task<StockAverageRateDto> CalculateAverageRateAsync(Guid tenantId, CalculateStockAverageRateRequest request);
    Task<bool> IsBinRequiredAsync();
}
