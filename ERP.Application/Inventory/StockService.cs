using ERP.Application.Interfaces;
using ERP.Shared.DTOs.Inventory;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Domain.SCM;

namespace ERP.Application.Inventory;

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

    public async Task<StockBalanceDto?> GetByItemAndWarehouseAsync(Guid itemId, Guid warehouseId)
    {
        var entity = await _repository.GetByItemAndWarehouseAsync(itemId, warehouseId, Guid.Empty);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<decimal> GetStockLevelAsync(Guid itemId, Guid warehouseId)
    {
        // This method needs to be implemented in the repository
        throw new NotImplementedException("GetStockLevelAsync method needs to be implemented in the repository");
    }

    public async Task<bool> AdjustStockAsync(StockAdjustmentDto dto)
    {
        // This method needs to be implemented in the repository
        throw new NotImplementedException("AdjustStockAsync method needs to be implemented in the repository");
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

    public async Task<bool> TransferStockAsync(StockTransferDto dto)
    {
        // This method needs to be implemented in the repository
        throw new NotImplementedException("TransferStockAsync method needs to be implemented in the repository");
    }

    public async Task<IEnumerable<StockLedgerDto>> GetStockLedgerAsync(Guid itemId, Guid warehouseId)
    {
        // This method needs to be implemented in the repository
        throw new NotImplementedException("GetStockLedgerAsync method needs to be implemented in the repository");
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
            QuantityAvailable = entity.AvailableQuantity,
            LastUpdated = entity.LastUpdated,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            IsActive = entity.IsActive
        };
    }

    private static StockLedgerDto MapToDto(StockLedger entity)
    {
        return new StockLedgerDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            ItemId = entity.ItemId,
            ItemName = entity.Item?.ItemName ?? string.Empty,
            WarehouseId = entity.WarehouseId,
            WarehouseName = entity.Warehouse?.WarehouseName ?? string.Empty,
            TransactionType = entity.TransactionType,
            Quantity = entity.QuantityIn - entity.QuantityOut,
            Reference = entity.ReferenceDocType,
            TransactionDate = entity.TransactionDate,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy
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