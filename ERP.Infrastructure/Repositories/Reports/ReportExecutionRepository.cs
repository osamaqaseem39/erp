using ERP.Domain.Reports;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Reports;

public class ReportExecutionRepository : IReportExecutionRepository
{
    private readonly ERPDbContext _context;

    public ReportExecutionRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<ReportExecution?> GetByIdAsync(Guid id)
    {
        return await _context.ReportExecutions
            .Include(re => re.Report)
            .Include(re => re.Schedule)
            .FirstOrDefaultAsync(re => re.Id == id);
    }

    public async Task<IEnumerable<ReportExecution>> GetAllAsync()
    {
        return await _context.ReportExecutions
            .Include(re => re.Report)
            .Include(re => re.Schedule)
            .OrderByDescending(re => re.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReportExecution>> GetByReportAsync(Guid reportId)
    {
        return await _context.ReportExecutions
            .Include(re => re.Report)
            .Include(re => re.Schedule)
            .Where(re => re.ReportId == reportId)
            .OrderByDescending(re => re.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReportExecution>> GetByReportIdAsync(Guid reportId)
    {
        return await _context.ReportExecutions
            .Include(re => re.Report)
            .Include(re => re.Schedule)
            .Where(re => re.ReportId == reportId)
            .OrderByDescending(re => re.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReportExecution>> GetByStatusAsync(string status)
    {
        return await _context.ReportExecutions
            .Include(re => re.Report)
            .Include(re => re.Schedule)
            .Where(re => re.Status == status)
            .OrderByDescending(re => re.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReportExecution>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.ReportExecutions
            .Include(re => re.Report)
            .Include(re => re.Schedule)
            .Where(re => re.StartedAt >= startDate && re.StartedAt <= endDate)
            .OrderByDescending(re => re.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReportExecution>> GetByTenantIdAsync(Guid tenantId)
    {
        return await _context.ReportExecutions
            .Include(re => re.Report)
            .Include(re => re.Schedule)
            .Where(re => re.TenantId == tenantId)
            .OrderByDescending(re => re.StartedAt)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(ReportExecution reportExecution)
    {
        _context.ReportExecutions.Add(reportExecution);
        await _context.SaveChangesAsync();
        return reportExecution.Id;
    }

    public async Task<bool> UpdateAsync(ReportExecution reportExecution)
    {
        _context.ReportExecutions.Update(reportExecution);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var reportExecution = await _context.ReportExecutions.FindAsync(id);
        if (reportExecution == null) return false;

        _context.ReportExecutions.Remove(reportExecution);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}