using ERP.Domain.Admin;
using ERP.Shared.DTOs.Admin;

namespace ERP.Api.Mappers.Admin;

public class SystemSettingMapper : BaseMapper<SystemSetting, SystemSettingDto>
{
    public override SystemSettingDto Map(SystemSetting source)
    {
        if (source == null) return null!;

        return new SystemSettingDto
        {
            Id = source.Id,
            SettingKey = source.SettingKey,
            SettingValue = source.SettingValue,
            Description = source.Description,
            SettingType = source.SettingType,
            Category = source.Category,
            IsEncrypted = source.IsEncrypted,
            IsReadOnly = source.IsReadOnly,
            ValidationRule = source.ValidationRule,
            Notes = source.Notes,
            TenantId = source.TenantId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public override SystemSetting Map(SystemSettingDto destination)
    {
        if (destination == null) return null!;

        return new SystemSetting
        {
            Id = destination.Id ?? Guid.NewGuid(),
            SettingKey = destination.SettingKey,
            SettingValue = destination.SettingValue,
            Description = destination.Description,
            SettingType = destination.SettingType,
            Category = destination.Category,
            IsEncrypted = destination.IsEncrypted,
            IsReadOnly = destination.IsReadOnly,
            ValidationRule = destination.ValidationRule,
            Notes = destination.Notes,
            TenantId = destination.TenantId != Guid.Empty ? destination.TenantId : Guid.Empty,
            CreatedAt = destination.CreatedAt,
            UpdatedAt = destination.UpdatedAt
        };
    }
}

