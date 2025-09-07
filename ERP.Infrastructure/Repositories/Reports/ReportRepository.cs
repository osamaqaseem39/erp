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

    public async Task<Report?> GetByIdAsync(Guid id)
    {
        return await _context.Reports
            .Include(r => r.ReportSchedules)
            .Include(r => r.ReportExecutions)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Report>> GetAllAsync()
    {
        return await _context.Reports
            .Include(r => r.ReportSchedules)
            .Include(r => r.ReportExecutions)
            .ToListAsync();
    }

    public async Task<IEnumerable<Report>> GetByTypeAsync(string reportType)
    {
        return await _context.Reports
            .Include(r => r.ReportSchedules)
            .Include(r => r.ReportExecutions)
            .Where(r => r.ReportType == reportType)
            .ToListAsync();
    }

    public async Task<IEnumerable<Report>> GetByCategoryAsync(string category)
    {
        return await _context.Reports
            .Include(r => r.ReportSchedules)
            .Include(r => r.ReportExecutions)
            .Where(r => r.Category == category)
            .ToListAsync();
    }

    public async Task<IEnumerable<Report>> GetActiveAsync()
    {
        return await _context.Reports
            .Include(r => r.ReportSchedules)
            .Include(r => r.ReportExecutions)
            .Where(r => r.IsActive)
            .ToListAsync();
    }

    public async Task<Report?> GetByCodeAsync(string reportCode)
    {
        return await _context.Reports
            .Include(r => r.ReportSchedules)
            .Include(r => r.ReportExecutions)
            .FirstOrDefaultAsync(r => r.ReportCode == reportCode);
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

    public async Task<bool> DeleteAsync(Guid id)
    {
        var report = await _context.Reports.FindAsync(id);
        if (report == null) return false;

        _context.Reports.Remove(report);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}