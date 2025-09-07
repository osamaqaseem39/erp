using ERP.Domain.Admin;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Admin;

public class SystemLogRepository : ISystemLogRepository
{
    private readonly ERPDbContext _context;

    public SystemLogRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<SystemLog?> GetByIdAsync(Guid id)
    {
        return await _context.SystemLogs.FindAsync(id);
    }

    public async Task<IEnumerable<SystemLog>> GetAllAsync()
    {
        return await _context.SystemLogs
            .OrderByDescending(sl => sl.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<SystemLog>> GetByLevelAsync(string logLevel)
    {
        return await _context.SystemLogs
            .Where(sl => sl.LogLevel == logLevel)
            .OrderByDescending(sl => sl.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<SystemLog>> GetByUserAsync(Guid userId)
    {
        return await _context.SystemLogs
            .Where(sl => sl.UserId == userId)
            .OrderByDescending(sl => sl.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<SystemLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.SystemLogs
            .Where(sl => sl.Timestamp >= startDate && sl.Timestamp <= endDate)
            .OrderByDescending(sl => sl.Timestamp)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(SystemLog systemLog)
    {
        _context.SystemLogs.Add(systemLog);
        await _context.SaveChangesAsync();
        return systemLog.Id;
    }

    public async Task<bool> UpdateAsync(SystemLog systemLog)
    {
        _context.SystemLogs.Update(systemLog);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var systemLog = await _context.SystemLogs.FindAsync(id);
        if (systemLog == null) return false;

        _context.SystemLogs.Remove(systemLog);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}