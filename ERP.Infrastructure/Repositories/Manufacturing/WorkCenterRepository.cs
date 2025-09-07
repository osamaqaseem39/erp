using ERP.Domain.Manufacturing;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Manufacturing;

public class WorkCenterRepository : IWorkCenterRepository
{
    private readonly ERPDbContext _context;

    public WorkCenterRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<WorkCenter?> GetByIdAsync(Guid id)
    {
        return await _context.WorkCenters
            .Include(wc => wc.ProductionRuns)
            .FirstOrDefaultAsync(wc => wc.Id == id);
    }

    public async Task<IEnumerable<WorkCenter>> GetAllAsync()
    {
        return await _context.WorkCenters
            .Include(wc => wc.ProductionRuns)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkCenter>> GetActiveAsync()
    {
        return await _context.WorkCenters
            .Include(wc => wc.ProductionRuns)
            .Where(wc => wc.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkCenter>> GetByTypeAsync(string workCenterType)
    {
        return await _context.WorkCenters
            .Include(wc => wc.ProductionRuns)
            .Where(wc => wc.WorkCenterType == workCenterType)
            .ToListAsync();
    }

    public async Task<WorkCenter?> GetByCodeAsync(string workCenterCode)
    {
        return await _context.WorkCenters
            .Include(wc => wc.ProductionRuns)
            .FirstOrDefaultAsync(wc => wc.WorkCenterCode == workCenterCode);
    }

    public async Task<Guid> CreateAsync(WorkCenter workCenter)
    {
        _context.WorkCenters.Add(workCenter);
        await _context.SaveChangesAsync();
        return workCenter.Id;
    }

    public async Task<bool> UpdateAsync(WorkCenter workCenter)
    {
        _context.WorkCenters.Update(workCenter);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var workCenter = await _context.WorkCenters.FindAsync(id);
        if (workCenter == null) return false;

        _context.WorkCenters.Remove(workCenter);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}