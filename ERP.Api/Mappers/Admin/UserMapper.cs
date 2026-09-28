using ERP.Domain.Core;
using ERP.Shared.DTOs.Admin;

namespace ERP.Api.Mappers.Admin;

public class UserMapper : BaseMapper<User, UserDto>
{
    public override UserDto Map(User source)
    {
        if (source == null) return null!;

        return new UserDto
        {
            Id = source.Id,
            FirstName = source.FirstName ?? string.Empty,
            LastName = source.LastName ?? string.Empty,
            Email = source.Email,
            Phone = source.PhoneNumber,
            IsActive = source.IsActive,
            TenantId = source.TenantId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public override User Map(UserDto destination)
    {
        if (destination == null) return null!;

        return new User
        {
            Id = destination.Id,
            FirstName = destination.FirstName,
            LastName = destination.LastName,
            Email = destination.Email,
            PhoneNumber = destination.Phone,
            IsActive = destination.IsActive,
            TenantId = destination.TenantId != Guid.Empty ? destination.TenantId : Guid.Empty,
            CreatedAt = destination.CreatedAt,
            UpdatedAt = destination.UpdatedAt
        };
    }
}

