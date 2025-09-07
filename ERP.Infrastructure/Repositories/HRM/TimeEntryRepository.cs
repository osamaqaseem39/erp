using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.HRM;

public class TimeEntryRepository : ITimeEntryRepository
{
    private readonly ERPDbContext _context;

    public TimeEntryRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<TimeEntry?> GetByIdAsync(Guid id)
    {
        return await _context.TimeEntries
            .Include(t => t.Employee)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<TimeEntry>> GetAllAsync()
    {
        return await _context.TimeEntries
            .Include(t => t.Employee)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeEntry>> GetByEmployeeAsync(Guid employeeId)
    {
        return await _context.TimeEntries
            .Include(t => t.Employee)
            .Where(t => t.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeEntry>> GetByProjectAsync(Guid projectId)
    {
        return await _context.TimeEntries
            .Include(t => t.Employee)
            .Where(t => t.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeEntry>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.TimeEntries
            .Include(t => t.Employee)
            .Where(t => t.Date >= startDate && t.Date <= endDate)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(TimeEntry timeEntry)
    {
        _context.TimeEntries.Add(timeEntry);
        await _context.SaveChangesAsync();
        return timeEntry.Id;
    }

    public async Task<bool> UpdateAsync(TimeEntry timeEntry)
    {
        _context.TimeEntries.Update(timeEntry);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var timeEntry = await _context.TimeEntries.FindAsync(id);
        if (timeEntry == null) return false;

        _context.TimeEntries.Remove(timeEntry);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}