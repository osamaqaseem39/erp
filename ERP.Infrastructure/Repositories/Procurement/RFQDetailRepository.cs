using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class RFQDetailRepository : IRFQDetailRepository
{
    private readonly ERPDbContext _context;

    public RFQDetailRepository(ERPDbContext context) => _context = context;

    private IQueryable<RFQDetail> Query(Guid tenantId) =>
        _context.RFQDetails.Where(d => d.TenantId == tenantId);

    public async Task<RFQDetail?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await Query(tenantId).FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<RFQDetail>> GetByRfqIdAsync(Guid rfqId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<RFQDetail>();
        return await Query(tenantId).Where(d => d.RFQId == rfqId).ToListAsync();
    }

    public async Task<Guid> CreateAsync(RFQDetail entity)
    {
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        _context.RFQDetails.Add(entity);
        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(RFQDetail entity)
    {
        _context.RFQDetails.Update(entity);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        _context.RFQDetails.Remove(entity);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteByRfqIdAsync(Guid rfqId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var rows = await Query(tenantId).Where(d => d.RFQId == rfqId).ToListAsync();
        if (rows.Count == 0) return true;
        _context.RFQDetails.RemoveRange(rows);
        return await _context.SaveChangesAsync() > 0;
    }
}
