using ERP.Domain.Admin;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Admin;

public class SystemSettingRepository : ISystemSettingRepository
{
    private readonly ERPDbContext _context;

    public SystemSettingRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<SystemSetting?> GetByIdAsync(Guid id)
    {
        return await _context.SystemSettings.FindAsync(id);
    }

    public async Task<IEnumerable<SystemSetting>> GetAllAsync()
    {
        return await _context.SystemSettings.ToListAsync();
    }

    public async Task<IEnumerable<SystemSetting>> GetByCategoryAsync(string category)
    {
        return await _context.SystemSettings
            .Where(s => s.Category == category)
            .ToListAsync();
    }

    public async Task<SystemSetting?> GetByKeyAsync(string settingKey)
    {
        return await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.SettingKey == settingKey);
    }

    public async Task<Guid> CreateAsync(SystemSetting systemSetting)
    {
        _context.SystemSettings.Add(systemSetting);
        await _context.SaveChangesAsync();
        return systemSetting.Id;
    }

    public async Task<bool> UpdateAsync(SystemSetting systemSetting)
    {
        _context.SystemSettings.Update(systemSetting);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var systemSetting = await _context.SystemSettings.FindAsync(id);
        if (systemSetting == null) return false;

        _context.SystemSettings.Remove(systemSetting);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}