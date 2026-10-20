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

    // Avoid Include on required Employee — missing rows become INNER JOINs and hide entries.
    private IQueryable<TimeEntry> Query(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return _context.TimeEntries.Where(_ => false);
        return _context.TimeEntries.Where(t => t.TenantId == tenantId);
    }

    private async Task LoadEmployeeAsync(TimeEntry? entity)
    {
        if (entity == null) return;
        await _context.Entry(entity).Reference(t => t.Employee).LoadAsync();
    }

    private async Task LoadEmployeesAsync(IEnumerable<TimeEntry> entities)
    {
        foreach (var entity in entities)
            await _context.Entry(entity).Reference(t => t.Employee).LoadAsync();
    }

    public async Task<TimeEntry?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await Query(tenantId).FirstOrDefaultAsync(t => t.Id == id);
        await LoadEmployeeAsync(entity);
        return entity;
    }

    public async Task<IEnumerable<TimeEntry>> GetAllAsync(Guid tenantId)
    {
        var list = await Query(tenantId).ToListAsync();
        await LoadEmployeesAsync(list);
        return list;
    }

    public async Task<IEnumerable<TimeEntry>> GetByEmployeeAsync(Guid employeeId, Guid tenantId)
    {
        var list = await Query(tenantId).Where(t => t.EmployeeId == employeeId).ToListAsync();
        await LoadEmployeesAsync(list);
        return list;
    }

    public async Task<IEnumerable<TimeEntry>> GetByProjectAsync(Guid projectId, Guid tenantId)
    {
        var list = await Query(tenantId).Where(t => t.ProjectId == projectId).ToListAsync();
        await LoadEmployeesAsync(list);
        return list;
    }

    public async Task<IEnumerable<TimeEntry>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId)
    {
        var list = await Query(tenantId)
            .Where(t => t.Date >= startDate && t.Date <= endDate)
            .ToListAsync();
        await LoadEmployeesAsync(list);
        return list;
    }

    public async Task<IEnumerable<TimeEntry>> GetByEmployeeAndDateRangeAsync(
        Guid employeeId, DateTime startDate, DateTime endDate, Guid tenantId)
    {
        var list = await Query(tenantId)
            .Where(t => t.EmployeeId == employeeId && t.Date >= startDate && t.Date <= endDate)
            .ToListAsync();
        await LoadEmployeesAsync(list);
        return list;
    }

    public async Task<Guid> CreateAsync(TimeEntry timeEntry)
    {
        if (timeEntry.Id == Guid.Empty) timeEntry.Id = Guid.NewGuid();
        if (timeEntry.CreatedAt == default) timeEntry.CreatedAt = DateTime.UtcNow;
        _context.TimeEntries.Add(timeEntry);
        await _context.SaveChangesAsync();
        return timeEntry.Id;
    }

    public async Task<bool> UpdateAsync(TimeEntry timeEntry)
    {
        timeEntry.UpdatedAt = DateTime.UtcNow;
        _context.TimeEntries.Update(timeEntry);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var timeEntry = await GetByIdAsync(id, tenantId);
        if (timeEntry == null) return false;
        _context.TimeEntries.Remove(timeEntry);
        return await _context.SaveChangesAsync() > 0;
    }
}
