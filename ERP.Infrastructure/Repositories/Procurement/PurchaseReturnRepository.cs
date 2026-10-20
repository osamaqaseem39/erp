using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class PurchaseReturnRepository : IPurchaseReturnRepository
{
    private readonly ERPDbContext _context;

    public PurchaseReturnRepository(ERPDbContext context) => _context = context;

    private IQueryable<PurchaseReturn> Query(Guid tenantId) =>
        _context.PurchaseReturns
            .Include(r => r.Vendor)
            .Include(r => r.Lines)
            .Where(r => r.TenantId == tenantId);

    public async Task<PurchaseReturn?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await Query(tenantId).FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<PurchaseReturn>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<PurchaseReturn>();
        return await Query(tenantId).OrderByDescending(r => r.ReturnDate).ToListAsync();
    }

    public async Task<Guid> CreateAsync(PurchaseReturn entity)
    {
        _context.PurchaseReturns.Add(entity);
        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(PurchaseReturn entity)
    {
        _context.PurchaseReturns.Update(entity);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        _context.PurchaseReturns.Remove(entity);
        return await _context.SaveChangesAsync() > 0;
    }
}
