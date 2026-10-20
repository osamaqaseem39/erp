using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Interfaces.Inventory;

public interface IStockAdjustmentService
{
    Task<StockAdjustmentDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<StockAdjustmentDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateStockAdjustmentDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateStockAdjustmentDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> PostAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId);
    Task<IEnumerable<StockAdjustmentDto>> GetByItemAsync(Guid itemId, Guid tenantId);
    Task<IEnumerable<StockAdjustmentDto>> GetByWarehouseAsync(Guid warehouseId, Guid tenantId);
    Task<IEnumerable<StockAdjustmentDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
}
