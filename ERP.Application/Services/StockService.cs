using ERP.Application.Interfaces;
using ERP.Shared.DTOs.Inventory;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Domain.SCM;

namespace ERP.Application.Services;

public class StockService : IStockService
{
    private readonly IStockBalanceRepository _repository;

    public StockService(IStockBalanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<StockBalanceDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id, Guid.Empty);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<StockBalanceDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync(Guid.Empty);
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(StockBalanceDto dto)
    {
        var entity = MapToEntity(dto);
        var created = await _repository.CreateAsync(entity);
        return created.Id;
    }

    public async Task<bool> UpdateAsync(StockBalanceDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id, dto.TenantId);
        if (entity == null) return false;

        MapToEntity(dto, entity);
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id, Guid.Empty);
    }

    public async Task<StockBalanceDto?> GetByItemAndWarehouseAsync(Guid itemId, Guid warehouseId)
    {
        var entity = await _repository.GetByItemAndWarehouseAsync(itemId, warehouseId, Guid.Empty);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<StockBalanceDto>> GetByItemAsync(Guid itemId)
    {
        var entities = await _repository.GetByItemAsync(itemId, Guid.Empty);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<StockBalanceDto>> GetByWarehouseAsync(Guid warehouseId)
    {
        var entities = await _repository.GetByWarehouseAsync(warehouseId, Guid.Empty);
        return entities.Select(MapToDto);
    }

    public async Task<decimal> GetStockLevelAsync(Guid itemId, Guid warehouseId)
    {
        var stockBalance = await _repository.GetByItemAndWarehouseAsync(itemId, warehouseId, Guid.Empty);
        return stockBalance?.CurrentQuantity ?? 0;
    }

    public async Task<bool> AdjustStockAsync(StockAdjustmentDto dto)
    {
        // Process each detail in the stock adjustment
        foreach (var detail in dto.Details)
        {
            var stockBalance = await _repository.GetByItemAndWarehouseAsync(detail.ItemId, dto.WarehouseId, dto.TenantId);
            
            if (stockBalance == null)
            {
                // Create new stock balance if it doesn't exist
                stockBalance = new StockBalance
                {
                    Id = Guid.NewGuid(),
                    TenantId = dto.TenantId,
                    ItemId = detail.ItemId,
                    WarehouseId = dto.WarehouseId,
                    CurrentQuantity = detail.QuantityCounted,
                    ReservedQuantity = 0,
                    LastUpdated = DateTime.UtcNow,
                    CreatedBy = dto.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                };
                await _repository.CreateAsync(stockBalance);
            }
            else
            {
                // Update existing stock balance with the difference
                stockBalance.CurrentQuantity += detail.QuantityDifference;
                stockBalance.LastUpdated = DateTime.UtcNow;
                stockBalance.UpdatedBy = dto.UpdatedBy;
                stockBalance.UpdatedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(stockBalance);
            }
        }

        return true;
    }

    private static StockBalanceDto MapToDto(StockBalance entity)
    {
        return new StockBalanceDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            ItemId = entity.ItemId,
            ItemName = entity.Item?.ItemName ?? string.Empty,
            WarehouseId = entity.WarehouseId,
            WarehouseName = entity.Warehouse?.WarehouseName ?? string.Empty,
            QuantityOnHand = entity.CurrentQuantity,
            QuantityReserved = entity.ReservedQuantity,
            QuantityAvailable = entity.CurrentQuantity - entity.ReservedQuantity,
            LastUpdated = entity.LastUpdated,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            IsActive = entity.IsActive
        };
    }

    private static StockBalance MapToEntity(StockBalanceDto dto)
    {
        return new StockBalance
        {
            Id = dto.Id,
            TenantId = dto.TenantId,
            ItemId = dto.ItemId,
            WarehouseId = dto.WarehouseId,
            CurrentQuantity = dto.QuantityOnHand,
            ReservedQuantity = dto.QuantityReserved,
            LastUpdated = dto.LastUpdated,
            CreatedBy = dto.CreatedBy
        };
    }

    private static void MapToEntity(StockBalanceDto dto, StockBalance entity)
    {
        entity.ItemId = dto.ItemId;
        entity.WarehouseId = dto.WarehouseId;
        entity.CurrentQuantity = dto.QuantityOnHand;
        entity.ReservedQuantity = dto.QuantityReserved;
        entity.LastUpdated = dto.LastUpdated;
        entity.UpdatedBy = dto.UpdatedBy;
    }
}