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
        var warehouse = await _warehouseRepository.GetByIdAsync(id, tenantId);
        return warehouse != null ? MapToDto(warehouse) : null;
    }

    public async Task<IEnumerable<WarehouseDto>> GetAllAsync(Guid tenantId, Guid companyId)
    {
        var warehouses = await _warehouseRepository.GetAllAsync(tenantId, companyId);
        return warehouses.Select(MapToDto);
    }

    public async Task<WarehouseDto> CreateAsync(CreateWarehouseDto dto, Guid tenantId, string createdBy)
    {
        if (await _warehouseRepository.CodeExistsAsync(dto.WarehouseCode, tenantId, dto.CompanyId))
        {
            throw new InvalidOperationException($"Warehouse with code '{dto.WarehouseCode}' already exists.");
        }

        var warehouse = new Warehouse
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CompanyId = dto.CompanyId,
            WarehouseCode = dto.WarehouseCode,
            WarehouseName = dto.WarehouseName,
            AddressLine1 = dto.AddressLine1,
            AddressLine2 = dto.AddressLine2,
            City = dto.City,
            State = dto.State,
            Country = dto.Country,
            PostalCode = dto.PostalCode,
            ContactPerson = dto.ContactPerson,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            CreatedBy = createdBy
        };

        var createdWarehouse = await _warehouseRepository.AddAsync(warehouse);
        return MapToDto(createdWarehouse);
    }

    public async Task<WarehouseDto> UpdateAsync(UpdateWarehouseDto dto, Guid tenantId, string updatedBy)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(dto.Id, tenantId);
        if (warehouse == null)
        {
            throw new InvalidOperationException("Warehouse not found.");
        }

        if (await _warehouseRepository.CodeExistsAsync(dto.WarehouseCode, tenantId, warehouse.CompanyId, dto.Id))
        {
            throw new InvalidOperationException($"Warehouse with code '{dto.WarehouseCode}' already exists.");
        }

        warehouse.WarehouseCode = dto.WarehouseCode;
        warehouse.WarehouseName = dto.WarehouseName;
        warehouse.AddressLine1 = dto.AddressLine1;
        warehouse.AddressLine2 = dto.AddressLine2;
        warehouse.City = dto.City;
        warehouse.State = dto.State;
        warehouse.Country = dto.Country;
        warehouse.PostalCode = dto.PostalCode;
        warehouse.ContactPerson = dto.ContactPerson;
        warehouse.PhoneNumber = dto.PhoneNumber;
        warehouse.Email = dto.Email;
        warehouse.IsActive = dto.IsActive ?? warehouse.IsActive;
        warehouse.UpdatedBy = updatedBy;
        warehouse.UpdatedAt = DateTime.UtcNow;

        var updatedWarehouse = await _warehouseRepository.UpdateAsync(warehouse);
        return MapToDto(updatedWarehouse);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        return await _warehouseRepository.DeleteAsync(id, tenantId);
    }

    public async Task<bool> ExistsAsync(Guid id, Guid tenantId)
    {
        return await _warehouseRepository.ExistsAsync(id, tenantId);
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
            AddressLine1 = warehouse.AddressLine1,
            AddressLine2 = warehouse.AddressLine2,
            City = warehouse.City,
            State = warehouse.State,
            Country = warehouse.Country,
            PostalCode = warehouse.PostalCode,
            ContactPerson = warehouse.ContactPerson,
            PhoneNumber = warehouse.PhoneNumber,
            Email = warehouse.Email,
            IsActive = warehouse.IsActive,
            CreatedAt = warehouse.CreatedAt,
            UpdatedAt = warehouse.UpdatedAt,
            CreatedBy = warehouse.CreatedBy,
            UpdatedBy = warehouse.UpdatedBy
        };
    }
}