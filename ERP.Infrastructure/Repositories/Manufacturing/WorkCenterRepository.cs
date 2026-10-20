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

    private IQueryable<WorkCenter> Query(Guid tenantId) =>
        _context.WorkCenters
            .Include(wc => wc.ProductionRuns)
            .Where(wc => wc.TenantId == tenantId);

    public async Task<WorkCenter?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await Query(tenantId).FirstOrDefaultAsync(wc => wc.Id == id);
    }

    public async Task<IEnumerable<WorkCenter>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<WorkCenter>();
        return await Query(tenantId).ToListAsync();
    }

    public async Task<IEnumerable<WorkCenter>> GetActiveAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<WorkCenter>();
        return await Query(tenantId).Where(wc => wc.IsActive).ToListAsync();
    }

    public async Task<IEnumerable<WorkCenter>> GetByTypeAsync(string workCenterType, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<WorkCenter>();
        return await Query(tenantId).Where(wc => wc.WorkCenterType == workCenterType).ToListAsync();
    }

    public async Task<WorkCenter?> GetByCodeAsync(string workCenterCode, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await Query(tenantId).FirstOrDefaultAsync(wc => wc.WorkCenterCode == workCenterCode);
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

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var workCenter = await GetByIdAsync(id, tenantId);
        if (workCenter == null) return false;

        _context.WorkCenters.Remove(workCenter);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}
