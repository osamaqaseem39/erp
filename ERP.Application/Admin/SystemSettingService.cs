using ERP.Application.Interfaces.Admin;
using ERP.Infrastructure.Repositories.Admin;
using ERP.Shared.DTOs.Admin;
using ERP.Domain.Admin;

namespace ERP.Application.Admin;

public class SystemSettingService : ISystemSettingService
{
    private readonly ISystemSettingRepository _systemSettingRepository;

    public SystemSettingService(ISystemSettingRepository systemSettingRepository)
    {
        _systemSettingRepository = systemSettingRepository;
    }

    public async Task<SystemSettingDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var setting = await _systemSettingRepository.GetByIdAsync(id);
        return setting != null ? MapToDto(setting) : null;
    }

    public async Task<SystemSettingDto?> GetByKeyAsync(string key, Guid tenantId)
    {
        var setting = await _systemSettingRepository.GetByKeyAsync(key);
        return setting != null ? MapToDto(setting) : null;
    }

    public async Task<IEnumerable<SystemSettingDto>> GetAllAsync(Guid tenantId, string? category = null)
    {
        var settings = await _systemSettingRepository.GetAllAsync();
        
        if (category != null)
            settings = settings.Where(s => s.Category == category);

        return settings.Select(MapToDto);
    }

    public async Task<IEnumerable<SystemSettingDto>> GetByCategoryAsync(string category, Guid tenantId)
    {
        var settings = await _systemSettingRepository.GetByCategoryAsync(category);
        return settings.Select(MapToDto);
    }

    public async Task<SystemSettingDto> CreateAsync(CreateSystemSettingDto dto, Guid tenantId, Guid userId)
    {
        var setting = new SystemSetting
        {
            Id = Guid.NewGuid(),
            SettingKey = dto.SettingKey,
            SettingValue = dto.SettingValue,
            Description = dto.Description,
            Category = dto.Category,
            SettingType = dto.SettingType,
            IsEncrypted = dto.IsEncrypted,
            TenantId = tenantId,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        var createdId = await _systemSettingRepository.CreateAsync(setting);
        var created = await _systemSettingRepository.GetByIdAsync(createdId);
        return MapToDto(created!);
    }

    public async Task<SystemSettingDto> UpdateAsync(UpdateSystemSettingDto dto, Guid tenantId, Guid userId)
    {
        var setting = await _systemSettingRepository.GetByIdAsync(dto.Id);
        if (setting == null)
            throw new KeyNotFoundException($"System Setting with ID '{dto.Id}' not found.");

        setting.SettingValue = dto.SettingValue;
        setting.Description = dto.Description;
        setting.UpdatedBy = userId.ToString();
        setting.UpdatedAt = DateTime.UtcNow;

        await _systemSettingRepository.UpdateAsync(setting);
        return MapToDto(setting);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        return await _systemSettingRepository.DeleteAsync(id);
    }

    public async Task<SystemSettingDto> UpsertAsync(UpsertSystemSettingDto dto, Guid tenantId, Guid userId)
    {
        var existingSetting = await _systemSettingRepository.GetByKeyAsync(dto.SettingKey);
        
        if (existingSetting != null)
        {
            var updateDto = new UpdateSystemSettingDto
            {
                Id = existingSetting.Id,
                SettingValue = dto.SettingValue,
                Description = dto.Description
            };
            return await UpdateAsync(updateDto, tenantId, userId);
        }
        else
        {
            var createDto = new CreateSystemSettingDto
            {
                SettingKey = dto.SettingKey,
                SettingValue = dto.SettingValue,
                Description = dto.Description,
                Category = dto.Category,
                SettingType = dto.SettingType,
                IsEncrypted = dto.IsEncrypted
            };
            return await CreateAsync(createDto, tenantId, userId);
        }
    }

    private static SystemSettingDto MapToDto(SystemSetting setting)
    {
        return new SystemSettingDto
        {
            Id = setting.Id,
            SettingKey = setting.SettingKey,
            SettingValue = setting.SettingValue,
            Description = setting.Description,
            Category = setting.Category,
            SettingType = setting.SettingType,
            IsEncrypted = setting.IsEncrypted
        };
    }
}