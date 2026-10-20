using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Finance;

public class CostCenterRepository : ICostCenterRepository
{
    private readonly ERPDbContext _context;

    public CostCenterRepository(ERPDbContext context)
    {
        _context = context;
    }

    private IQueryable<CostCenter> ForTenant(Guid tenantId)
        => tenantId == Guid.Empty
            ? _context.CostCenters.Where(_ => false)
            : _context.CostCenters.Where(cc => cc.TenantId == tenantId);

    public async Task<CostCenter?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await ForTenant(tenantId)
            .FirstOrDefaultAsync(cc => cc.CostCenterId == id || cc.Id == id);
    }

    public async Task<IEnumerable<CostCenter>> GetAllAsync(Guid tenantId, bool? isActive = null)
    {
        if (tenantId == Guid.Empty) return Array.Empty<CostCenter>();
        var q = ForTenant(tenantId);
        if (isActive.HasValue) q = q.Where(cc => cc.IsActive == isActive.Value);
        return await q.OrderBy(cc => cc.CostCenterCode).ToListAsync();
    }

    public async Task<CostCenter?> GetByCodeAsync(string costCenterCode, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await ForTenant(tenantId)
            .FirstOrDefaultAsync(cc => cc.CostCenterCode == costCenterCode);
    }

    public async Task<bool> IsReferencedByJournalEntriesAsync(Guid costCenterId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        return await _context.JournalEntries
            .AnyAsync(e => e.CostCenterId == costCenterId && e.TenantId == tenantId);
    }

    public async Task<Guid> CreateAsync(CostCenter costCenter)
    {
        if (costCenter.TenantId == Guid.Empty) return Guid.Empty;
        if (costCenter.Id == Guid.Empty) costCenter.Id = Guid.NewGuid();
        if (costCenter.CostCenterId == Guid.Empty) costCenter.CostCenterId = costCenter.Id;
        costCenter.CreatedAt = DateTime.UtcNow;
        _context.CostCenters.Add(costCenter);
        await _context.SaveChangesAsync();
        return costCenter.CostCenterId;
    }

    public async Task<bool> UpdateAsync(CostCenter costCenter)
    {
        if (costCenter.TenantId == Guid.Empty) return false;
        costCenter.UpdatedAt = DateTime.UtcNow;
        _context.CostCenters.Update(costCenter);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> SoftDeactivateAsync(Guid id, Guid tenantId)
    {
        var costCenter = await GetByIdAsync(id, tenantId);
        if (costCenter == null) return false;
        costCenter.IsActive = false;
        costCenter.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var costCenter = await GetByIdAsync(id, tenantId);
        if (costCenter == null) return false;
        if (await IsReferencedByJournalEntriesAsync(costCenter.CostCenterId, tenantId))
            return false;
        _context.CostCenters.Remove(costCenter);
        return await _context.SaveChangesAsync() > 0;
    }
}
