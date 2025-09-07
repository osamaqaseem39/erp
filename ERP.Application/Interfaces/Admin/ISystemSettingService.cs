using ERP.Shared.DTOs.Admin;

namespace ERP.Application.Interfaces.Admin;

public interface ISystemSettingService
{
    Task<SystemSettingDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<SystemSettingDto?> GetByKeyAsync(string key, Guid tenantId);
    Task<IEnumerable<SystemSettingDto>> GetAllAsync(Guid tenantId, string? category = null);
    Task<IEnumerable<SystemSettingDto>> GetByCategoryAsync(string category, Guid tenantId);
    Task<SystemSettingDto> CreateAsync(CreateSystemSettingDto dto, Guid tenantId, Guid userId);
    Task<SystemSettingDto> UpdateAsync(UpdateSystemSettingDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<SystemSettingDto> UpsertAsync(UpsertSystemSettingDto dto, Guid tenantId, Guid userId);
}