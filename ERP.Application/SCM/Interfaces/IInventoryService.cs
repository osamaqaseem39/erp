using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.SCM.Interfaces;

public interface IInventoryService
{
    Task<StockBalanceDto?> GetStockBalanceAsync(Guid itemId, Guid warehouseId, Guid tenantId);
    Task<IEnumerable<StockBalanceDto>> GetAllStockBalancesAsync(Guid tenantId, Guid companyId, Guid? warehouseId = null);
    Task<IEnumerable<StockLedgerDto>> GetStockLedgerAsync(Guid itemId, Guid warehouseId, Guid tenantId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<bool> AdjustInventoryAsync(Guid itemId, Guid warehouseId, Guid tenantId, string adjustmentType, decimal quantity, string reason, string? approvedBy, string createdBy);
    Task<bool> TransferStockAsync(CreateStockTransferDto dto, Guid tenantId, string createdBy);
    Task<bool> UpdateTransferStatusAsync(Guid transferId, Guid tenantId, string status, string? approvedBy, string updatedBy);
    Task<IEnumerable<StockTransferDto>> GetStockTransfersAsync(Guid tenantId, Guid companyId);
    Task<StockTransferDto?> GetStockTransferByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<StockBalanceDto>> GetLowStockItemsAsync(Guid tenantId, Guid companyId);
    Task<InventoryValuationDto> GetInventoryValuationAsync(Guid tenantId, Guid? warehouseId = null, DateTime? asOfDate = null);
}