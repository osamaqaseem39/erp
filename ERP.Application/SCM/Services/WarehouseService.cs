using ERP.Application.SCM.Interfaces;
using ERP.Domain.SCM;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.SCM.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _warehouseRepository;

    public WarehouseService(IWarehouseRepository warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }

    public async Task<WarehouseDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return null;

        var warehouse = await _warehouseRepository.GetByIdAsync(id, tenantId);
        return warehouse != null ? MapToDto(warehouse) : null;
    }

    public async Task<IEnumerable<WarehouseDto>> GetAllAsync(Guid tenantId, Guid companyId)
    {
        if (tenantId == Guid.Empty)
            return Array.Empty<WarehouseDto>();

        var warehouses = await _warehouseRepository.GetAllAsync(tenantId, companyId);
        return warehouses.Select(MapToDto);
    }

    public async Task<IEnumerable<WarehouseDto>> SearchAsync(string searchTerm, Guid tenantId, Guid companyId = default)
    {
        if (tenantId == Guid.Empty)
            return Array.Empty<WarehouseDto>();

        var term = searchTerm?.Trim() ?? string.Empty;
        var all = await _warehouseRepository.GetAllAsync(tenantId, companyId);
        if (string.IsNullOrEmpty(term))
            return all.Select(MapToDto);

        return all.Where(w =>
                w.WarehouseCode.Contains(term, StringComparison.OrdinalIgnoreCase)
                || w.WarehouseName.Contains(term, StringComparison.OrdinalIgnoreCase)
                || (w.City?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false))
            .Select(MapToDto);
    }

    public async Task<WarehouseDto> CreateAsync(CreateWarehouseDto dto, Guid tenantId, string createdBy)
    {
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Tenant is required.");

        if (await _warehouseRepository.CodeExistsAsync(dto.WarehouseCode, tenantId, dto.CompanyId))
            throw new InvalidOperationException($"Warehouse with code '{dto.WarehouseCode}' already exists.");

        var warehouse = new Warehouse
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
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
            ContactPerson = dto.ContactPerson,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            ManagerId = dto.ManagerId,
            IsActive = dto.IsActive,
            CreatedBy = createdBy
        };

        var createdWarehouse = await _warehouseRepository.AddAsync(warehouse);
        return MapToDto(createdWarehouse);
    }

    public async Task<WarehouseDto> UpdateAsync(UpdateWarehouseDto dto, Guid tenantId, string updatedBy)
    {
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Tenant is required.");

        var warehouse = await _warehouseRepository.GetByIdAsync(dto.Id, tenantId)
            ?? throw new KeyNotFoundException($"Warehouse '{dto.Id}' not found.");

        if (!string.IsNullOrWhiteSpace(dto.WarehouseCode)
            && await _warehouseRepository.CodeExistsAsync(dto.WarehouseCode, tenantId, warehouse.CompanyId, dto.Id))
            throw new InvalidOperationException($"Warehouse with code '{dto.WarehouseCode}' already exists.");

        if (!string.IsNullOrWhiteSpace(dto.WarehouseCode))
            warehouse.WarehouseCode = dto.WarehouseCode;
        if (!string.IsNullOrWhiteSpace(dto.WarehouseName))
        {
            warehouse.WarehouseName = dto.WarehouseName;
            warehouse.Name = dto.WarehouseName;
        }
        if (dto.Description != null) warehouse.Description = dto.Description;
        if (dto.AddressLine1 != null) warehouse.AddressLine1 = dto.AddressLine1;
        if (dto.AddressLine2 != null) warehouse.AddressLine2 = dto.AddressLine2;
        if (dto.City != null) warehouse.City = dto.City;
        if (dto.State != null) warehouse.State = dto.State;
        if (dto.Country != null) warehouse.Country = dto.Country;
        if (dto.PostalCode != null) warehouse.PostalCode = dto.PostalCode;
        if (dto.ContactPerson != null) warehouse.ContactPerson = dto.ContactPerson;
        if (dto.PhoneNumber != null) warehouse.PhoneNumber = dto.PhoneNumber;
        if (dto.Email != null) warehouse.Email = dto.Email;
        if (dto.ManagerId.HasValue) warehouse.ManagerId = dto.ManagerId;
        if (dto.IsActive.HasValue) warehouse.IsActive = dto.IsActive.Value;
        warehouse.UpdatedBy = updatedBy;
        warehouse.UpdatedAt = DateTime.UtcNow;

        var updatedWarehouse = await _warehouseRepository.UpdateAsync(warehouse);
        return MapToDto(updatedWarehouse);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return false;

        return await _warehouseRepository.DeleteAsync(id, tenantId);
    }

    public async Task<bool> ExistsAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return false;

        return await _warehouseRepository.ExistsAsync(id, tenantId);
    }

    public async Task<bool> ActivateAsync(Guid id, Guid tenantId, string updatedBy)
    {
        if (tenantId == Guid.Empty)
            return false;

        var warehouse = await _warehouseRepository.GetByIdAsync(id, tenantId);
        if (warehouse == null) return false;
        if (warehouse.IsActive) return true;
        warehouse.IsActive = true;
        warehouse.UpdatedBy = updatedBy;
        warehouse.UpdatedAt = DateTime.UtcNow;
        await _warehouseRepository.UpdateAsync(warehouse);
        return true;
    }

    public async Task<bool> DeactivateAsync(Guid id, Guid tenantId, string updatedBy)
    {
        if (tenantId == Guid.Empty)
            return false;

        var warehouse = await _warehouseRepository.GetByIdAsync(id, tenantId);
        if (warehouse == null) return false;
        if (!warehouse.IsActive) return true;
        warehouse.IsActive = false;
        warehouse.UpdatedBy = updatedBy;
        warehouse.UpdatedAt = DateTime.UtcNow;
        await _warehouseRepository.UpdateAsync(warehouse);
        return true;
    }

    private static WarehouseDto MapToDto(Warehouse warehouse)
    {
        return new WarehouseDto
        {
            Id = warehouse.Id,
            TenantId = warehouse.TenantId,
            CompanyId = warehouse.CompanyId,
            WarehouseCode = warehouse.WarehouseCode,
            WarehouseName = warehouse.WarehouseName,
            Description = warehouse.Description,
            AddressLine1 = warehouse.AddressLine1,
            AddressLine2 = warehouse.AddressLine2,
            City = warehouse.City,
            State = warehouse.State,
            Country = warehouse.Country,
            PostalCode = warehouse.PostalCode,
            ContactPerson = warehouse.ContactPerson,
            PhoneNumber = warehouse.PhoneNumber,
            Email = warehouse.Email,
            ManagerId = warehouse.ManagerId,
            IsActive = warehouse.IsActive,
            CreatedAt = warehouse.CreatedAt,
            UpdatedAt = warehouse.UpdatedAt,
            CreatedBy = warehouse.CreatedBy,
            UpdatedBy = warehouse.UpdatedBy
        };
    }
}
