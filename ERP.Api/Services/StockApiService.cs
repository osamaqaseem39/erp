using ERP.Application.Interfaces;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Api.Services;

public class StockApiService : IStockApiService
{
    private readonly IStockService _service;

    public StockApiService(IStockService service)
    {
        _service = service;
    }

    public async Task<StockBalanceDto?> GetByIdAsync(Guid id)
    {
        return await _service.GetByIdAsync(id);
    }

    public async Task<IEnumerable<StockBalanceDto>> GetAllAsync()
    {
        return await _service.GetAllAsync();
    }

    public async Task<IEnumerable<StockBalanceDto>> GetByItemAsync(Guid itemId)
    {
        return await _service.GetByItemAsync(itemId);
    }

    public async Task<IEnumerable<StockBalanceDto>> GetByWarehouseAsync(Guid warehouseId)
    {
        return await _service.GetByWarehouseAsync(warehouseId);
    }

    public async Task<decimal> GetStockLevelAsync(Guid itemId, Guid warehouseId)
    {
        return await _service.GetStockLevelAsync(itemId, warehouseId);
    }

    public async Task<bool> AdjustStockAsync(StockAdjustmentDto dto)
    {
        return await _service.AdjustStockAsync(dto);
    }

    public Task<bool> TransferStockAsync(StockTransferDto dto)
        => _service.TransferStockAsync(dto);

    public Task<IEnumerable<StockLedgerDto>> GetStockLedgerAsync(Guid itemId, Guid warehouseId)
        => _service.GetStockLedgerAsync(itemId, Guid.Empty, warehouseId);
}