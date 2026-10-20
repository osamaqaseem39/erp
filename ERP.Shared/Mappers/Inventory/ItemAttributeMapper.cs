using ERP.Domain.SCM;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Shared.Mappers.Inventory;

public static class ItemAttributeMapper
{
    public static ItemAttributeDto ToDto(this ItemAttribute entity)
    {
        return new ItemAttributeDto
        {
            Id = entity.Id,
            AttributeName = entity.AttributeName,
            AttributeType = entity.AttributeType,
            IsRequired = entity.IsRequired,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public static ItemAttribute ToEntity(this ItemAttributeDto dto)
    {
        return new ItemAttribute
        {
            Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            AttributeName = dto.AttributeName,
            AttributeType = dto.AttributeType,
            IsRequired = dto.IsRequired,
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }
}