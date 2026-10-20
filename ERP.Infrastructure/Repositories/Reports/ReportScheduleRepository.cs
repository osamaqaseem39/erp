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

    private IQueryable<ReportSchedule> Query(Guid tenantId) =>
        _context.ReportSchedules
            .Include(rs => rs.Report)
            .Where(rs => rs.TenantId == tenantId);

    public async Task<ReportSchedule?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await Query(tenantId).FirstOrDefaultAsync(rs => rs.Id == id);
    }

    public async Task<IEnumerable<ReportSchedule>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<ReportSchedule>();
        return await Query(tenantId).ToListAsync();
    }

    public async Task<IEnumerable<ReportSchedule>> GetByReportAsync(Guid reportId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<ReportSchedule>();
        return await Query(tenantId).Where(rs => rs.ReportId == reportId).ToListAsync();
    }

    public async Task<IEnumerable<ReportSchedule>> GetActiveAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<ReportSchedule>();
        return await Query(tenantId).Where(rs => rs.IsActive).ToListAsync();
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
        => await GetAllAsync(tenantId);

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

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var reportSchedule = await GetByIdAsync(id, tenantId);
        if (reportSchedule == null) return false;

        _context.ReportSchedules.Remove(reportSchedule);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}
