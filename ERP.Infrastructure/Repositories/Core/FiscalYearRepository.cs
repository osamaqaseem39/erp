using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Core;

public class FiscalYearRepository : IFiscalYearRepository
{
    private readonly ERPDbContext _db;

    public FiscalYearRepository(ERPDbContext db) => _db = db;

    public async Task<FiscalYear?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await _db.FiscalYears
            .FirstOrDefaultAsync(f => f.TenantId == tenantId && (f.FiscalYearId == id || f.Id == id));
    }

    public async Task<IEnumerable<FiscalYear>> GetAllAsync(Guid tenantId, string? status = null)
    {
        if (tenantId == Guid.Empty) return Array.Empty<FiscalYear>();
        var q = _db.FiscalYears.Where(f => f.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(f => f.Status == status.Trim());
        return await q.OrderByDescending(f => f.StartDate).ToListAsync();
    }

    public async Task<FiscalYear?> FindActiveCoveringAsync(Guid tenantId, DateTime date)
    {
        if (tenantId == Guid.Empty) return null;
        var d = date.Date;
        return await _db.FiscalYears
            .Where(f => f.TenantId == tenantId
                && f.Status == "Active"
                && f.StartDate <= d
                && f.EndDate >= d)
            .OrderByDescending(f => f.StartDate)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> HasAnyActiveAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        return await _db.FiscalYears.AnyAsync(f => f.TenantId == tenantId && f.Status == "Active");
    }

    public async Task<bool> HasOverlapAsync(Guid tenantId, DateTime start, DateTime end, Guid? excludeId = null)
    {
        if (tenantId == Guid.Empty) return false;
        var s = start.Date;
        var e = end.Date;
        var q = _db.FiscalYears.Where(f =>
            f.TenantId == tenantId
            && f.StartDate <= e
            && f.EndDate >= s);
        if (excludeId.HasValue && excludeId != Guid.Empty)
            q = q.Where(f => f.FiscalYearId != excludeId.Value && f.Id != excludeId.Value);
        return await q.AnyAsync();
    }

    public async Task<Guid> CreateAsync(FiscalYear entity)
    {
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        if (entity.FiscalYearId == Guid.Empty) entity.FiscalYearId = entity.Id;
        entity.CreatedAt = DateTime.UtcNow;
        _db.FiscalYears.Add(entity);
        await _db.SaveChangesAsync();
        return entity.FiscalYearId;
    }

    public async Task<bool> UpdateAsync(FiscalYear entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _db.FiscalYears.Update(entity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity == null) return false;

        var referenced = await _db.Journals.AnyAsync(j =>
            j.TenantId == tenantId && j.FinancialYearId == entity.FiscalYearId);
        if (referenced) return false;

        _db.FiscalYears.Remove(entity);
        return await _db.SaveChangesAsync() > 0;
    }
}
