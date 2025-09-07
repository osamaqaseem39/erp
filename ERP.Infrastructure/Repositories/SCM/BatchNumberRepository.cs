using ERP.Domain.SCM;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.SCM;

public class BatchNumberRepository : IBatchNumberRepository
{
    private readonly ERPDbContext _context;

    public BatchNumberRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BatchNumber>> GetByItemAsync(Guid itemId, Guid tenantId)
    {
        return await _context.Set<BatchNumber>()
            .Include(bn => bn.Item)
            .Where(bn => bn.ItemId == itemId && bn.TenantId == tenantId && bn.IsActive)
            .ToListAsync();
    }

    public async Task<BatchNumber?> GetByIdAsync(Guid id, Guid tenantId)
    {
        return await _context.Set<BatchNumber>()
            .Include(bn => bn.Item)
            .FirstOrDefaultAsync(bn => bn.Id == id && bn.TenantId == tenantId);
    }

    public async Task<BatchNumber> CreateAsync(BatchNumber entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _context.Set<BatchNumber>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<BatchNumber> UpdateAsync(BatchNumber entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Set<BatchNumber>().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _context.Set<BatchNumber>()
            .FirstOrDefaultAsync(bn => bn.Id == id && bn.TenantId == tenantId);

        if (entity == null)
            return false;

        _context.Set<BatchNumber>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}