using ERP.Domain.SCM;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Shared.Mappers.Inventory;

public static class InventoryValuationMethodMapper
{
    public static InventoryValuationMethodDto ToDto(this InventoryValuationMethod entity)
    {
        return new InventoryValuationMethodDto
        {
            Id = entity.Id,
            MethodName = entity.MethodName,
            MethodCode = entity.MethodCode,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public static InventoryValuationMethod ToEntity(this InventoryValuationMethodDto dto)
    {
        return new InventoryValuationMethod
        {
            Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            MethodName = dto.MethodName,
            MethodCode = dto.MethodCode,
            Description = dto.Description,
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }
}