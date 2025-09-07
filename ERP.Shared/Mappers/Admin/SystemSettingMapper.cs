using ERP.Domain.Admin;
using ERP.Shared.DTOs.Admin;
using ERP.Shared.Mappers;

namespace ERP.Shared.Mappers.Admin;

public static class SystemSettingMapper
{
    public static SystemSettingDto ToDto(SystemSetting entity)
    {
        return new SystemSettingDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            SettingKey = entity.SettingKey,
            SettingValue = entity.SettingValue,
            SettingType = entity.SettingType,
            Description = entity.Description,
            IsEncrypted = entity.IsEncrypted,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static SystemSetting ToEntity(CreateSystemSettingDto dto, Guid tenantId, string createdBy)
    {
        return new SystemSetting
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SettingKey = dto.SettingKey,
            SettingValue = dto.SettingValue,
            SettingType = dto.SettingType,
            Description = dto.Description,
            IsEncrypted = dto.IsEncrypted,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static void UpdateEntity(UpdateSystemSettingDto dto, SystemSetting entity, string updatedBy)
    {
        entity.SettingValue = dto.SettingValue;
        entity.SettingType = dto.SettingType ?? entity.SettingType;
        entity.Description = dto.Description;
        entity.IsEncrypted = dto.IsEncrypted ?? false;
        entity.IsActive = dto.IsActive ?? true;
        
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;
    }

    public static void UpsertEntity(UpsertSystemSettingDto dto, SystemSetting entity, string updatedBy)
    {
        entity.SettingValue = dto.SettingValue;
        entity.SettingType = dto.SettingType;
        entity.Description = dto.Description;
        entity.IsEncrypted = dto.IsEncrypted;
        entity.IsActive = dto.IsActive ?? true;
        
        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.CreatedBy = updatedBy;
        }
        else
        {
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = updatedBy;
        }
    }
}