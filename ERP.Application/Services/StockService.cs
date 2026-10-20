using ERP.Application.Interfaces;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Services;

/// <summary>DI facade over <see cref="Inventory.StockService"/>.</summary>
public class StockService : IStockService
{
    private readonly Inventory.StockService _inner;

    public StockService(
        IStockBalanceRepository repository,
        IStockLedgerRepository ledgers,
        ERPDbContext db,
        ERP.Application.Core.Settings.ICoreSettingsService? settings = null)
    {
        _inner = new Inventory.StockService(repository, ledgers, db, settings: settings);
    }

    public Task<StockBalanceDto?> GetByIdAsync(Guid id) => _inner.GetByIdAsync(id);
    public Task<StockBalanceDto?> GetByIdAsync(Guid id, Guid tenantId) => _inner.GetByIdAsync(id, tenantId);
    public Task<IEnumerable<StockBalanceDto>> GetAllAsync() => _inner.GetAllAsync();
    public Task<IEnumerable<StockBalanceDto>> GetAllAsync(Guid tenantId) => _inner.GetAllAsync(tenantId);
    public Task<Guid> CreateAsync(StockBalanceDto dto) => _inner.CreateAsync(dto);
    public Task<Guid> CreateAsync(StockBalanceDto dto, Guid tenantId) => _inner.CreateAsync(dto, tenantId);
    public Task<bool> UpdateAsync(StockBalanceDto dto) => _inner.UpdateAsync(dto);
    public Task<bool> UpdateAsync(StockBalanceDto dto, Guid tenantId) => _inner.UpdateAsync(dto, tenantId);
    public Task<bool> DeleteAsync(Guid id) => _inner.DeleteAsync(id);
    public Task<bool> DeleteAsync(Guid id, Guid tenantId) => _inner.DeleteAsync(id, tenantId);
    public Task<StockBalanceDto?> GetByItemAndWarehouseAsync(Guid itemId, Guid warehouseId)
        => _inner.GetByItemAndWarehouseAsync(itemId, warehouseId);
    public Task<StockBalanceDto?> GetByItemAndWarehouseAsync(Guid itemId, Guid warehouseId, Guid tenantId)
        => _inner.GetByItemAndWarehouseAsync(itemId, warehouseId, tenantId);
    public Task<StockBalanceDto?> GetByItemWarehouseLocationAsync(Guid itemId, Guid warehouseId, Guid? warehouseLocationId, Guid tenantId)
        => _inner.GetByItemWarehouseLocationAsync(itemId, warehouseId, warehouseLocationId, tenantId);
    public Task<IEnumerable<StockBalanceDto>> GetByItemAsync(Guid itemId) => _inner.GetByItemAsync(itemId);
    public Task<IEnumerable<StockBalanceDto>> GetByItemAsync(Guid itemId, Guid tenantId)
        => _inner.GetByItemAsync(itemId, tenantId);
    public Task<IEnumerable<StockBalanceDto>> GetByWarehouseAsync(Guid warehouseId)
        => _inner.GetByWarehouseAsync(warehouseId);
    public Task<IEnumerable<StockBalanceDto>> GetByWarehouseAsync(Guid warehouseId, Guid tenantId)
        => _inner.GetByWarehouseAsync(warehouseId, tenantId);
    public Task<decimal> GetStockLevelAsync(Guid itemId, Guid warehouseId)
        => _inner.GetStockLevelAsync(itemId, warehouseId);
    public Task<decimal> GetStockLevelAsync(Guid itemId, Guid warehouseId, Guid tenantId)
        => _inner.GetStockLevelAsync(itemId, warehouseId, tenantId);
    public Task<bool> AdjustStockAsync(StockAdjustmentDto dto) => _inner.AdjustStockAsync(dto);
    public Task<bool> AdjustStockAsync(StockAdjustmentDto dto, Guid tenantId)
        => _inner.AdjustStockAsync(dto, tenantId);
    public Task<bool> TransferStockAsync(StockTransferDto dto) => _inner.TransferStockAsync(dto);
    public Task<bool> ReceiveStockAsync(Guid tenantId, Guid warehouseId, IEnumerable<(Guid ItemId, decimal Quantity)> lines, string? userId = null)
        => _inner.ReceiveStockAsync(tenantId, warehouseId, lines, userId);
    public Task<bool> ReceiveStockAsync(Guid tenantId, Guid warehouseId, IEnumerable<(Guid ItemId, decimal Quantity, decimal UnitCost)> lines, string? userId = null)
        => _inner.ReceiveStockAsync(tenantId, warehouseId, lines, userId);
    public Task<bool> ReceiveStockAsync(Guid tenantId, Guid warehouseId, IEnumerable<(Guid ItemId, decimal Quantity, Guid? WarehouseLocationId)> lines, string? userId = null)
        => _inner.ReceiveStockAsync(tenantId, warehouseId, lines, userId);
    public Task<bool> IssueStockAsync(Guid tenantId, Guid warehouseId, IEnumerable<(Guid ItemId, decimal Quantity)> lines, string? userId = null)
        => _inner.IssueStockAsync(tenantId, warehouseId, lines, userId);
    public Task<bool> IssueStockAsync(Guid tenantId, Guid warehouseId, IEnumerable<(Guid ItemId, decimal Quantity, Guid? WarehouseLocationId)> lines, string? userId = null)
        => _inner.IssueStockAsync(tenantId, warehouseId, lines, userId);
    public Task<bool> IssueStockAnyWarehouseAsync(Guid tenantId, IEnumerable<(Guid ItemId, decimal Quantity)> lines, string? userId = null)
        => _inner.IssueStockAnyWarehouseAsync(tenantId, lines, userId);
    public Task<bool> ReserveAsync(Guid tenantId, Guid warehouseId, Guid itemId, decimal quantity, string? userId = null)
        => _inner.ReserveAsync(tenantId, warehouseId, itemId, quantity, userId);
    public Task<bool> UnreserveAsync(Guid tenantId, Guid warehouseId, Guid itemId, decimal quantity, string? userId = null)
        => _inner.UnreserveAsync(tenantId, warehouseId, itemId, quantity, userId);
    public Task<IEnumerable<StockLedgerDto>> GetStockLedgerAsync(Guid itemId, Guid tenantId, Guid? warehouseId = null, DateTime? toDate = null)
        => _inner.GetStockLedgerAsync(itemId, tenantId, warehouseId, toDate);
    public Task<StockAverageRateDto> CalculateAverageRateAsync(Guid tenantId, CalculateStockAverageRateRequest request)
        => _inner.CalculateAverageRateAsync(tenantId, request);
    public Task<bool> IsBinRequiredAsync() => _inner.IsBinRequiredAsync();
}
