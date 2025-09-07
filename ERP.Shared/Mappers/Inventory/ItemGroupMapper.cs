using ERP.Domain.SCM;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Shared.Mappers.Inventory;

public static class ItemGroupMapper
{
    public static ItemGroupDto ToDto(this ItemGroup entity)
    {
        return new ItemGroupDto
        {
            Id = entity.Id,
            GroupCode = entity.GroupCode,
            GroupName = entity.GroupName,
            ParentGroupId = entity.ParentGroupId,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            ParentGroupName = entity.ParentGroup?.GroupName
        };
    }

    public static ItemGroup ToEntity(this ItemGroupDto dto)
    {
        return new ItemGroup
        {
            Id = dto.Id,
            GroupCode = dto.GroupCode,
            GroupName = dto.GroupName,
            ParentGroupId = dto.ParentGroupId,
            Description = dto.Description,
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }
}