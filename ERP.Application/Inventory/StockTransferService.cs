using ERP.Application.Interfaces;
using ERP.Shared.DTOs.Inventory;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Domain.SCM;

namespace ERP.Application.Inventory;

public class StockTransferService : IStockTransferService
{
    private readonly IStockTransferRepository _repository;

    public StockTransferService(IStockTransferRepository repository)
    {
        _repository = repository;
    }

    public async Task<StockTransferDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id, Guid.Empty);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<StockTransferDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync(Guid.Empty);
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateStockTransferDto dto)
    {
        var entity = MapToEntity(dto);
        var created = await _repository.CreateAsync(entity);
        return created.Id;
    }

    public async Task<bool> UpdateAsync(UpdateStockTransferDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id, Guid.Empty);
        if (entity == null) return false;

        MapToEntity(dto, entity);
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id, Guid.Empty);
    }



    public async Task<IEnumerable<StockTransferDto>> GetByFromWarehouseAsync(Guid warehouseId)
    {
        var entities = await _repository.GetByWarehouseAsync(warehouseId, Guid.Empty);
        return entities.Where(e => e.FromWarehouseId == warehouseId).Select(MapToDto);
    }

    public async Task<IEnumerable<StockTransferDto>> GetByToWarehouseAsync(Guid warehouseId)
    {
        var entities = await _repository.GetByWarehouseAsync(warehouseId, Guid.Empty);
        return entities.Where(e => e.ToWarehouseId == warehouseId).Select(MapToDto);
    }

    public async Task<IEnumerable<StockTransferDto>> GetByWarehouseAsync(Guid warehouseId)
    {
        var entities = await _repository.GetByWarehouseAsync(warehouseId, Guid.Empty);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<StockTransferDto>> GetByStatusAsync(string status)
    {
        var entities = await _repository.GetByStatusAsync(status, Guid.Empty);
        return entities.Select(MapToDto);
    }

    public async Task<bool> ApproveAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id, Guid.Empty);
        if (entity == null) return false;

        entity.Status = "Approved";
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> CompleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id, Guid.Empty);
        if (entity == null) return false;

        entity.Status = "Completed";
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<Guid> CreateAsync(StockTransferDto dto)
    {
        var entity = MapToEntity(dto);
        var created = await _repository.CreateAsync(entity);
        return created.Id;
    }

    public async Task<bool> UpdateAsync(StockTransferDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id ?? Guid.Empty, dto.TenantId);
        if (entity == null) return false;

        MapToEntity(dto, entity);
        await _repository.UpdateAsync(entity);
        return true;
    }



    private static StockTransferDto MapToDto(StockTransfer entity)
    {
        return new StockTransferDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            TransferNumber = entity.Id.ToString(),
            FromWarehouseId = entity.FromWarehouseId,
            FromWarehouseName = entity.FromWarehouse?.WarehouseName ?? string.Empty,
            ToWarehouseId = entity.ToWarehouseId,
            ToWarehouseName = entity.ToWarehouse?.WarehouseName ?? string.Empty,
            TransferDate = entity.TransferDate,
            Status = entity.Status,
            Notes = string.Empty,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            IsActive = entity.IsActive
        };
    }

    private static StockTransfer MapToEntity(CreateStockTransferDto dto)
    {
        return new StockTransfer
        {
            TenantId = dto.TenantId,
            FromWarehouseId = dto.FromWarehouseId,
            ToWarehouseId = dto.ToWarehouseId,
            TransferDate = dto.TransferDate,
            Status = "Draft",
            CreatedBy = dto.CreatedBy
        };
    }

    private static void MapToEntity(UpdateStockTransferDto dto, StockTransfer entity)
    {
        entity.FromWarehouseId = dto.FromWarehouseId;
        entity.ToWarehouseId = dto.ToWarehouseId;
        entity.TransferDate = dto.TransferDate;
        entity.Status = dto.Status;
        entity.UpdatedBy = dto.UpdatedBy;
    }

    private static StockTransfer MapToEntity(StockTransferDto dto)
    {
        return new StockTransfer
        {
            Id = dto.Id ?? Guid.NewGuid(),
            TenantId = dto.TenantId,
            FromWarehouseId = dto.FromWarehouseId,
            ToWarehouseId = dto.ToWarehouseId,
            TransferDate = dto.TransferDate,
            Status = dto.Status,
            CreatedBy = dto.CreatedBy
        };
    }

    private static void MapToEntity(StockTransferDto dto, StockTransfer entity)
    {
        entity.FromWarehouseId = dto.FromWarehouseId;
        entity.ToWarehouseId = dto.ToWarehouseId;
        entity.TransferDate = dto.TransferDate;
        entity.Status = dto.Status;
        entity.UpdatedBy = dto.UpdatedBy;
    }


}