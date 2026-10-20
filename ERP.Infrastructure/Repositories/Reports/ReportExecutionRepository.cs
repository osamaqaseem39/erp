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

    private IQueryable<ReportExecution> Query(Guid tenantId) =>
        _context.ReportExecutions
            .Include(re => re.Report)
            .Include(re => re.Schedule)
            .Where(re => re.TenantId == tenantId);

    public async Task<ReportExecution?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await Query(tenantId).FirstOrDefaultAsync(re => re.Id == id);
    }

    public async Task<IEnumerable<ReportExecution>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<ReportExecution>();
        return await Query(tenantId).OrderByDescending(re => re.StartedAt).ToListAsync();
    }

    public async Task<IEnumerable<ReportExecution>> GetByReportAsync(Guid reportId, Guid tenantId)
        => await GetByReportIdAsync(reportId, tenantId);

    public async Task<IEnumerable<ReportExecution>> GetByReportIdAsync(Guid reportId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<ReportExecution>();
        return await Query(tenantId)
            .Where(re => re.ReportId == reportId)
            .OrderByDescending(re => re.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReportExecution>> GetByStatusAsync(string status, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<ReportExecution>();
        return await Query(tenantId)
            .Where(re => re.Status == status)
            .OrderByDescending(re => re.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReportExecution>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<ReportExecution>();
        return await Query(tenantId)
            .Where(re => re.StartedAt >= startDate && re.StartedAt <= endDate)
            .OrderByDescending(re => re.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReportExecution>> GetByTenantIdAsync(Guid tenantId)
        => await GetAllAsync(tenantId);

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

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var reportExecution = await GetByIdAsync(id, tenantId);
        if (reportExecution == null) return false;

        _context.ReportExecutions.Remove(reportExecution);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}
