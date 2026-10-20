using ERP.Application.Interfaces.Inventory;
using ERP.Domain.SCM;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Inventory;

public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _repository;

    public WarehouseService(IWarehouseRepository repository) => _repository = repository;

    public async Task<WarehouseDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<WarehouseDto>> GetAllAsync()
        => (await _repository.GetAllAsync()).Select(MapToDto);

    public async Task<IEnumerable<WarehouseDto>> GetActiveAsync()
        => (await _repository.GetAllAsync()).Where(w => w.IsActive).Select(MapToDto);

    public async Task<IEnumerable<WarehouseDto>> SearchAsync(string searchTerm)
    {
        var term = searchTerm?.Trim() ?? string.Empty;
        var all = await _repository.GetAllAsync();
        if (string.IsNullOrEmpty(term)) return all.Select(MapToDto);
        return all.Where(w =>
                w.WarehouseCode.Contains(term, StringComparison.OrdinalIgnoreCase)
                || w.WarehouseName.Contains(term, StringComparison.OrdinalIgnoreCase)
                || (w.City?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false))
            .Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateWarehouseDto dto)
    {
        // Tenant may be stamped by WarehouseRepositoryAdapter from ITenantProvider when dto.TenantId is empty.
        var entity = MapToEntity(dto);
        var created = await _repository.AddAsync(entity);
        return created.Id == Guid.Empty || created.TenantId == Guid.Empty ? Guid.Empty : created.Id;
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
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;
        await _repository.DeleteAsync(id);
        return true;
    }

    public async Task<bool> ActivateAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;
        if (entity.IsActive) return true;
        entity.IsActive = true;
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> DeactivateAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;
        if (!entity.IsActive) return true;
        entity.IsActive = false;
        await _repository.UpdateAsync(entity);
        return true;
    }

    private static WarehouseDto MapToDto(Warehouse entity) => new()
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

    private static Warehouse MapToEntity(CreateWarehouseDto dto) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = dto.TenantId,
        CompanyId = dto.CompanyId,
        WarehouseCode = dto.WarehouseCode,
        WarehouseName = dto.WarehouseName,
        Name = dto.WarehouseName,
        Description = dto.Description,
        AddressLine1 = dto.AddressLine1 ?? string.Empty,
        AddressLine2 = dto.AddressLine2,
        City = dto.City ?? string.Empty,
        State = dto.State ?? string.Empty,
        Country = dto.Country ?? string.Empty,
        PostalCode = dto.PostalCode ?? string.Empty,
        PhoneNumber = dto.PhoneNumber,
        Email = dto.Email,
        ManagerId = dto.ManagerId,
        ContactPerson = dto.ContactPerson,
        IsActive = true,
        CreatedBy = dto.CreatedBy,
        CreatedAt = DateTime.UtcNow
    };

    private static void MapToEntity(UpdateWarehouseDto dto, Warehouse entity)
    {
        if (!string.IsNullOrWhiteSpace(dto.WarehouseName))
        {
            entity.WarehouseName = dto.WarehouseName;
            entity.Name = dto.WarehouseName;
        }
        if (dto.Description != null) entity.Description = dto.Description;
        if (dto.AddressLine1 != null) entity.AddressLine1 = dto.AddressLine1;
        if (dto.AddressLine2 != null) entity.AddressLine2 = dto.AddressLine2;
        if (dto.City != null) entity.City = dto.City;
        if (dto.State != null) entity.State = dto.State;
        if (dto.Country != null) entity.Country = dto.Country;
        if (dto.PostalCode != null) entity.PostalCode = dto.PostalCode;
        if (dto.PhoneNumber != null) entity.PhoneNumber = dto.PhoneNumber;
        if (dto.Email != null) entity.Email = dto.Email;
        if (dto.ManagerId.HasValue) entity.ManagerId = dto.ManagerId;
        if (dto.ContactPerson != null) entity.ContactPerson = dto.ContactPerson;
        entity.UpdatedBy = dto.UpdatedBy;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
