using ERP.Application.Interfaces.Inventory;
using ERP.Shared.DTOs.Inventory;
using System;
using System.Linq;

namespace ERP.Application.Inventory;

/// <summary>DI stub for IStockAdjustmentService. Replace with real business logic incrementally.</summary>
public class StockAdjustmentService : IStockAdjustmentService
{
    public Task<StockAdjustmentDto?> GetByIdAsync(Guid id)
    {
        return Task.FromResult<StockAdjustmentDto?>(null);
    }

    public Task<IEnumerable<StockAdjustmentDto>> GetAllAsync()
    {
        return Task.FromResult(Enumerable.Empty<StockAdjustmentDto>());
    }

    public Task<Guid> CreateAsync(CreateStockAdjustmentDto dto)
    {
        return Task.FromResult(Guid.Empty);
    }

    public Task<bool> UpdateAsync(UpdateStockAdjustmentDto dto)
    {
        return Task.FromResult(false);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        return Task.FromResult(false);
    }

    public Task<IEnumerable<StockAdjustmentDto>> GetByItemAsync(Guid itemId)
    {
        return Task.FromResult(Enumerable.Empty<StockAdjustmentDto>());
    }

    public Task<IEnumerable<StockAdjustmentDto>> GetByWarehouseAsync(Guid warehouseId)
    {
        return Task.FromResult(Enumerable.Empty<StockAdjustmentDto>());
    }

    public Task<IEnumerable<StockAdjustmentDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return Task.FromResult(Enumerable.Empty<StockAdjustmentDto>());
    }

}
