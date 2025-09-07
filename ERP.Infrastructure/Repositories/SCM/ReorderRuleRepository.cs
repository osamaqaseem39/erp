using ERP.Domain.SCM;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.SCM;

public class ReorderRuleRepository : IReorderRuleRepository
{
    private readonly ERPDbContext _context;

    public ReorderRuleRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ReorderRule>> GetByItemAsync(Guid? itemId, Guid? warehouseId, Guid tenantId)
    {
        var query = _context.Set<ReorderRule>()
            .Include(rr => rr.Item)
            .Include(rr => rr.Warehouse)
            .Where(rr => rr.TenantId == tenantId && rr.IsActive);

        if (itemId.HasValue)
            query = query.Where(rr => rr.ItemId == itemId.Value);

        if (warehouseId.HasValue)
            query = query.Where(rr => rr.WarehouseId == warehouseId.Value);

        return await query.ToListAsync();
    }

    public async Task<ReorderRule?> GetByIdAsync(Guid id, Guid tenantId)
    {
        return await _context.Set<ReorderRule>()
            .Include(rr => rr.Item)
            .Include(rr => rr.Warehouse)
            .FirstOrDefaultAsync(rr => rr.Id == id && rr.TenantId == tenantId);
    }

    public async Task<ReorderRule> CreateAsync(ReorderRule entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _context.Set<ReorderRule>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<ReorderRule> UpdateAsync(ReorderRule entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Set<ReorderRule>().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _context.Set<ReorderRule>()
            .FirstOrDefaultAsync(rr => rr.Id == id && rr.TenantId == tenantId);

        if (entity == null)
            return false;

        _context.Set<ReorderRule>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}