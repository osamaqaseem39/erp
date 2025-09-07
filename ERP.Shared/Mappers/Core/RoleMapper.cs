using ERP.Domain.Core;
using ERP.Shared.DTOs.Core;
using ERP.Shared.Mappers;

namespace ERP.Shared.Mappers.Core;

public static class RoleMapper
{
    public static RoleDto ToDto(Role entity)
    {
        return new RoleDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            RoleName = entity.RoleName,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static Role ToEntity(CreateRoleDto dto, Guid tenantId, string createdBy)
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RoleName = dto.RoleName,
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static void UpdateEntity(UpdateRoleDto dto, Role entity, string updatedBy)
    {
        entity.RoleName = dto.RoleName;
        entity.Description = dto.Description;
        entity.IsActive = dto.IsActive;
        
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;
    }
}