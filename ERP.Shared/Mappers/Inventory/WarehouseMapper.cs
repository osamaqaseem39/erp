using ERP.Domain.SCM;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Shared.Mappers.Inventory;

public static class WarehouseMapper
{
    public static WarehouseDto ToDto(Warehouse warehouse)
    {
        return new WarehouseDto
        {
            Id = warehouse.Id,
            CompanyId = warehouse.CompanyId,
            WarehouseCode = warehouse.WarehouseCode,
            WarehouseName = warehouse.WarehouseName,
            Description = warehouse.Name,
            AddressLine1 = warehouse.AddressLine1,
            AddressLine2 = warehouse.AddressLine2,
            City = warehouse.City,
            State = warehouse.State,
            Country = warehouse.Country,
            PostalCode = warehouse.PostalCode,
            PhoneNumber = warehouse.PhoneNumber,
            Email = warehouse.Email,
            ManagerId = warehouse.ManagerId,
            ContactPerson = warehouse.ContactPerson,
            IsActive = warehouse.IsActive,
            TenantId = warehouse.TenantId,
            CreatedBy = warehouse.CreatedBy,
            CreatedAt = warehouse.CreatedAt,
            UpdatedBy = warehouse.UpdatedBy,
            UpdatedAt = warehouse.UpdatedAt
        };
    }

    public static Warehouse ToEntity(CreateWarehouseDto dto)
    {
        return new Warehouse
        {
            Id = Guid.NewGuid(),
            CompanyId = dto.CompanyId,
            WarehouseCode = dto.WarehouseCode,
            WarehouseName = dto.WarehouseName,
            Name = dto.Description,
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
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void UpdateEntity(UpdateWarehouseDto dto, Warehouse entity)
    {
        if (dto.WarehouseCode != null)
            entity.WarehouseCode = dto.WarehouseCode;
        if (dto.WarehouseName != null)
            entity.WarehouseName = dto.WarehouseName;
        if (dto.Description != null)
            entity.Name = dto.Description;
        if (dto.AddressLine1 != null)
            entity.AddressLine1 = dto.AddressLine1;
        if (dto.AddressLine2 != null)
            entity.AddressLine2 = dto.AddressLine2;
        if (dto.City != null)
            entity.City = dto.City;
        if (dto.State != null)
            entity.State = dto.State;
        if (dto.Country != null)
            entity.Country = dto.Country;
        if (dto.PostalCode != null)
            entity.PostalCode = dto.PostalCode;
        if (dto.PhoneNumber != null)
            entity.PhoneNumber = dto.PhoneNumber;
        if (dto.Email != null)
            entity.Email = dto.Email;
        if (dto.ManagerId != null)
            entity.ManagerId = dto.ManagerId;
        if (dto.ContactPerson != null)
            entity.ContactPerson = dto.ContactPerson;
        if (dto.IsActive.HasValue)
            entity.IsActive = dto.IsActive.Value;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}