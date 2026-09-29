using ERP.Domain.Admin;
using ERP.Infrastructure.Database;
using ERP.Shared.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Admin;

/// <summary>Settings of the current tenant only. Another tenant's ids behave as "not found".</summary>
public class SystemSettingRepository : TenantScopedRepositoryBase, ISystemSettingRepository
{
    public SystemSettingRepository(ERPDbContext context, ITenantProvider tenantProvider) : base(context, tenantProvider)
    {
    }

    private IQueryable<SystemSetting> Settings => Scoped(Context.SystemSettings);

    public async Task<SystemSetting?> GetByIdAsync(Guid id) => await Settings.FirstOrDefaultAsync(s => s.Id == id);

    public async Task<IEnumerable<SystemSetting>> GetAllAsync() =>
        await Settings.OrderBy(s => s.Category).ThenBy(s => s.SettingKey).ToListAsync();

    public async Task<IEnumerable<SystemSetting>> GetByCategoryAsync(string category) =>
        await Settings.Where(s => s.Category == category).OrderBy(s => s.SettingKey).ToListAsync();

    public async Task<SystemSetting?> GetByKeyAsync(string settingKey) =>
        await Settings.FirstOrDefaultAsync(s => s.SettingKey == settingKey);

    public async Task<Guid> CreateAsync(SystemSetting systemSetting)
    {
        Context.SystemSettings.Add(Owned(systemSetting));
        await Context.SaveChangesAsync();
        return systemSetting.Id;
    }

    public async Task<bool> UpdateAsync(SystemSetting systemSetting)
    {
        if (systemSetting.TenantId != CurrentTenantId) return false;
        Context.SystemSettings.Update(systemSetting);
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var setting = await GetByIdAsync(id);
        if (setting == null) return false;
        Context.SystemSettings.Remove(setting);
        return await Context.SaveChangesAsync() > 0;
    }
}
