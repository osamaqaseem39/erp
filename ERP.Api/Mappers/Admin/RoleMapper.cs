using ERP.Domain.Core;
using ERP.Shared.DTOs.Admin;

namespace ERP.Api.Mappers.Admin;

public class RoleMapper : BaseMapper<Role, RoleDto>
{
    public override RoleDto Map(Role source)
    {
        if (source == null) return null!;

        return new RoleDto
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            IsActive = source.IsActive,
            TenantId = source.TenantId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public override Role Map(RoleDto destination)
    {
        if (destination == null) return null!;

        return new Role
        {
            Id = destination.Id,
            Name = destination.Name,
            RoleName = destination.Name,
            Description = destination.Description,
            IsActive = destination.IsActive,
            TenantId = destination.TenantId != Guid.Empty ? destination.TenantId : Guid.Empty,
            CreatedAt = destination.CreatedAt,
            UpdatedAt = destination.UpdatedAt
        };
    }
}

