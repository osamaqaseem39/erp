using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.HRM;

public class HRSettingsRepository : IHRSettingsRepository
{
    private readonly ERPDbContext _context;

    public HRSettingsRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<HRSettings?> GetByIdAsync(Guid id)
    {
        return await _context.HRSettings.FindAsync(id);
    }

    public async Task<IEnumerable<HRSettings>> GetAllAsync()
    {
        return await _context.HRSettings.ToListAsync();
    }

    public async Task<HRSettings?> GetByTenantAsync(Guid tenantId)
    {
        return await _context.HRSettings
            .FirstOrDefaultAsync(h => h.TenantId == tenantId);
    }

    public async Task<Guid> CreateAsync(HRSettings hrSettings)
    {
        _context.HRSettings.Add(hrSettings);
        await _context.SaveChangesAsync();
        return hrSettings.Id;
    }

    public async Task<bool> UpdateAsync(HRSettings hrSettings)
    {
        _context.HRSettings.Update(hrSettings);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var hrSettings = await _context.HRSettings.FindAsync(id);
        if (hrSettings == null) return false;

        _context.HRSettings.Remove(hrSettings);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}