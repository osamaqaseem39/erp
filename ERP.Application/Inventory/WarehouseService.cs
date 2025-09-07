using ERP.Application.Interfaces;
using ERP.Shared.DTOs.Inventory;
using ERP.Infrastructure.Repositories;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Domain.SCM;

namespace ERP.Application.Inventory;

public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _repository;

    public WarehouseService(IWarehouseRepository repository)
    {
        _repository = repository;
    }

    public async Task<WarehouseDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<WarehouseDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateWarehouseDto dto)
    {
        var entity = MapToEntity(dto);
        var created = await _repository.AddAsync(entity);
        return created.Id;
    }

    public async Task<bool> UpdateAsync(UpdateWarehouseDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id);
        if (entity == null) return false;

        MapToEntity(dto, entity);
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
        return true;
    }

    public async Task<IEnumerable<WarehouseDto>> GetByLocationAsync(string location)
    {
        var entities = await _repository.GetAllAsync();
        return entities.Where(w => w.City == location).Select(MapToDto);
    }

    public async Task<IEnumerable<StockBalanceDto>> GetStockBalancesAsync(Guid warehouseId)
    {
        var entities = await _repository.GetAllAsync();
        return entities.Where(w => w.Id == warehouseId).SelectMany(w => w.StockBalances).Select(MapToDto);
    }

    public async Task<bool> TransferStockAsync(StockTransferDto dto)
    {
        // Implementation would go here
        return true;
    }

    private static WarehouseDto MapToDto(Warehouse entity)
    {
        return new WarehouseDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CompanyId = entity.CompanyId,
            WarehouseCode = entity.WarehouseCode,
            WarehouseName = entity.WarehouseName,
            Description = entity.Description,
            AddressLine1 = entity.AddressLine1,
            AddressLine2 = entity.AddressLine2,
            City = entity.City,
            State = entity.State,
            Country = entity.Country,
            PostalCode = entity.PostalCode,
            PhoneNumber = entity.PhoneNumber,
            Email = entity.Email,
            ManagerId = entity.ManagerId,
            ContactPerson = entity.ContactPerson,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy
        };
    }

    private static Warehouse MapToEntity(CreateWarehouseDto dto)
    {
        return new Warehouse
        {
            TenantId = dto.TenantId,
            CompanyId = dto.CompanyId,
            WarehouseCode = dto.WarehouseCode,
            WarehouseName = dto.WarehouseName,
            Description = dto.Description,
            AddressLine1 = dto.AddressLine1,
            AddressLine2 = dto.AddressLine2,
            City = dto.City,
            State = dto.State,
            Country = dto.Country,
            PostalCode = dto.PostalCode,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            ManagerId = dto.ManagerId,
            ContactPerson = dto.ContactPerson,
            CreatedBy = dto.CreatedBy
        };
    }

    private static void MapToEntity(UpdateWarehouseDto dto, Warehouse entity)
    {
        entity.WarehouseName = dto.WarehouseName;
        entity.Description = dto.Description;
        entity.AddressLine1 = dto.AddressLine1;
        entity.AddressLine2 = dto.AddressLine2;
        entity.City = dto.City;
        entity.State = dto.State;
        entity.Country = dto.Country;
        entity.PostalCode = dto.PostalCode;
        entity.PhoneNumber = dto.PhoneNumber;
        entity.Email = dto.Email;
        entity.ManagerId = dto.ManagerId;
        entity.ContactPerson = dto.ContactPerson;
        entity.UpdatedBy = dto.UpdatedBy;
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
}