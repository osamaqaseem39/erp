using ERP.Domain.Reports;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Reports;

public class ReportScheduleRepository : IReportScheduleRepository
{
    private readonly ERPDbContext _context;

    public ReportScheduleRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<ReportSchedule?> GetByIdAsync(Guid id)
    {
        return await _context.ReportSchedules
            .Include(rs => rs.Report)
            .FirstOrDefaultAsync(rs => rs.Id == id);
    }

    public async Task<IEnumerable<ReportSchedule>> GetAllAsync()
    {
        return await _context.ReportSchedules
            .Include(rs => rs.Report)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReportSchedule>> GetByReportAsync(Guid reportId)
    {
        return await _context.ReportSchedules
            .Include(rs => rs.Report)
            .Where(rs => rs.ReportId == reportId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReportSchedule>> GetActiveAsync()
    {
        return await _context.ReportSchedules
            .Include(rs => rs.Report)
            .Where(rs => rs.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReportSchedule>> GetScheduledForExecutionAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.ReportSchedules
            .Include(rs => rs.Report)
            .Where(rs => rs.IsActive && rs.NextRunTime <= now)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReportSchedule>> GetByTenantIdAsync(Guid tenantId)
    {
        return await _context.ReportSchedules
            .Include(rs => rs.Report)
            .Where(rs => rs.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(ReportSchedule reportSchedule)
    {
        _context.ReportSchedules.Add(reportSchedule);
        await _context.SaveChangesAsync();
        return reportSchedule.Id;
    }

    public async Task<bool> UpdateAsync(ReportSchedule reportSchedule)
    {
        _context.ReportSchedules.Update(reportSchedule);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var reportSchedule = await _context.ReportSchedules.FindAsync(id);
        if (reportSchedule == null) return false;

        _context.ReportSchedules.Remove(reportSchedule);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}