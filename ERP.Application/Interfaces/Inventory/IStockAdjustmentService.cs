using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Interfaces.Inventory;

public interface IStockAdjustmentService
{
    Task<StockAdjustmentDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<StockAdjustmentDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateStockAdjustmentDto dto);
    Task<bool> UpdateAsync(UpdateStockAdjustmentDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<StockAdjustmentDto>> GetByItemAsync(Guid itemId);
    Task<IEnumerable<StockAdjustmentDto>> GetByWarehouseAsync(Guid warehouseId);
    Task<IEnumerable<StockAdjustmentDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}