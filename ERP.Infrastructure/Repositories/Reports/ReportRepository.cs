using ERP.Domain.Reports;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Reports;

public class ReportRepository : IReportRepository
{
    private readonly ERPDbContext _context;

    public ReportRepository(ERPDbContext context)
    {
        _context = context;
    }

    private IQueryable<Report> Query(Guid tenantId) =>
        _context.Reports
            .Include(r => r.ReportSchedules)
            .Include(r => r.ReportExecutions)
            .Where(r => r.TenantId == tenantId);

    public async Task<Report?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await Query(tenantId).FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Report>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<Report>();
        return await Query(tenantId).ToListAsync();
    }

    public async Task<IEnumerable<Report>> GetByTypeAsync(string reportType, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<Report>();
        return await Query(tenantId).Where(r => r.ReportType == reportType).ToListAsync();
    }

    public async Task<IEnumerable<Report>> GetByCategoryAsync(string category, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<Report>();
        return await Query(tenantId).Where(r => r.Category == category).ToListAsync();
    }

    public async Task<IEnumerable<Report>> GetActiveAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<Report>();
        return await Query(tenantId).Where(r => r.IsActive).ToListAsync();
    }

    public async Task<Report?> GetByCodeAsync(string reportCode, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await Query(tenantId).FirstOrDefaultAsync(r => r.ReportCode == reportCode);
    }

    public async Task<Guid> CreateAsync(Report report)
    {
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();
        return report.Id;
    }

    public async Task<bool> UpdateAsync(Report report)
    {
        _context.Reports.Update(report);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var report = await GetByIdAsync(id, tenantId);
        if (report == null) return false;

        _context.Reports.Remove(report);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}
