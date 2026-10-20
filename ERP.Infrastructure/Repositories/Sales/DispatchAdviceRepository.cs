using ERP.Domain.Sales;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Sales;

public interface IDispatchAdviceRepository
{
    Task<DispatchAdvice?> GetByIdAsync(Guid id, Guid tenantId, bool includeLines = false);
    Task<IEnumerable<DispatchAdvice>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(DispatchAdvice entity);
    Task<bool> UpdateAsync(DispatchAdvice entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task ReplaceLinesAsync(Guid adviceId, Guid tenantId, IEnumerable<DispatchAdviceLine> lines);
}

public class DispatchAdviceRepository : IDispatchAdviceRepository
{
    private readonly ERPDbContext _db;
    public DispatchAdviceRepository(ERPDbContext db) => _db = db;

    public async Task<DispatchAdvice?> GetByIdAsync(Guid id, Guid tenantId, bool includeLines = false)
    {
        if (tenantId == Guid.Empty) return null;
        var q = _db.DispatchAdvices.AsQueryable();
        if (includeLines) q = q.Include(x => x.Lines);
        return await q.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
    }

    public async Task<IEnumerable<DispatchAdvice>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<DispatchAdvice>();
        return await _db.DispatchAdvices.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.AdviceDate)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(DispatchAdvice entity)
    {
        if (entity.TenantId == Guid.Empty) return Guid.Empty;
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        foreach (var line in entity.Lines)
        {
            if (line.Id == Guid.Empty) line.Id = Guid.NewGuid();
            line.TenantId = entity.TenantId;
            line.DispatchAdviceId = entity.Id;
            line.CreatedAt = DateTime.UtcNow;
        }
        _db.DispatchAdvices.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(DispatchAdvice entity)
    {
        if (entity.TenantId == Guid.Empty) return false;
        var existing = await _db.DispatchAdvices.FirstOrDefaultAsync(x => x.Id == entity.Id && x.TenantId == entity.TenantId);
        if (existing == null) return false;
        existing.SalesOrderId = entity.SalesOrderId;
        existing.CustomerId = entity.CustomerId;
        existing.WarehouseId = entity.WarehouseId;
        existing.AdviceDate = entity.AdviceDate;
        existing.Notes = entity.Notes;
        existing.Status = entity.Status;
        existing.DispatchedAt = entity.DispatchedAt;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = entity.UpdatedBy;
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _db.DispatchAdvices.Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (e == null) return false;
        _db.DispatchAdvices.Remove(e);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task ReplaceLinesAsync(Guid adviceId, Guid tenantId, IEnumerable<DispatchAdviceLine> lines)
    {
        var existing = await _db.DispatchAdviceLines
            .Where(x => x.DispatchAdviceId == adviceId && x.TenantId == tenantId).ToListAsync();
        _db.DispatchAdviceLines.RemoveRange(existing);
        foreach (var line in lines)
        {
            if (line.Id == Guid.Empty) line.Id = Guid.NewGuid();
            line.TenantId = tenantId;
            line.DispatchAdviceId = adviceId;
            line.CreatedAt = DateTime.UtcNow;
            _db.DispatchAdviceLines.Add(line);
        }
        await _db.SaveChangesAsync();
    }
}
