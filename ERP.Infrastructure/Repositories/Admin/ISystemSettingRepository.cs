using ERP.Domain.Admin;

namespace ERP.Infrastructure.Repositories.Admin;

public interface ISystemSettingRepository
{
    Task<SystemSetting?> GetByIdAsync(Guid id);
    Task<IEnumerable<SystemSetting>> GetAllAsync();
    Task<IEnumerable<SystemSetting>> GetByCategoryAsync(string category);
    Task<SystemSetting?> GetByKeyAsync(string settingKey);
    Task<Guid> CreateAsync(SystemSetting systemSetting);
    Task<bool> UpdateAsync(SystemSetting systemSetting);
    Task<bool> DeleteAsync(Guid id);
}