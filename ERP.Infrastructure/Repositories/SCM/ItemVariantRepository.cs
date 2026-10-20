using ERP.Domain.SCM;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.SCM;

public class ItemVariantRepository : IItemVariantRepository
{
    private readonly ERPDbContext _context;

    public ItemVariantRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ItemVariant>> GetAllAsync(Guid tenantId)
    {
        return await _context.Set<ItemVariant>()
            .Include(iv => iv.Item)
            .Where(iv => iv.TenantId == tenantId)
            .OrderBy(iv => iv.VariantCode)
            .ToListAsync();
    }

    public async Task<IEnumerable<ItemVariant>> GetByItemAsync(Guid itemId, Guid tenantId)
    {
        return await _context.Set<ItemVariant>()
            .Include(iv => iv.Item)
            .Where(iv => iv.ItemId == itemId && iv.TenantId == tenantId && iv.IsActive)
            .ToListAsync();
    }

    public async Task<ItemVariant?> GetByIdAsync(Guid id, Guid tenantId)
    {
        return await _context.Set<ItemVariant>()
            .Include(iv => iv.Item)
            .FirstOrDefaultAsync(iv => iv.Id == id && iv.TenantId == tenantId);
    }

    public async Task<ItemVariant> CreateAsync(ItemVariant entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _context.Set<ItemVariant>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<ItemVariant> UpdateAsync(ItemVariant entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Set<ItemVariant>().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _context.Set<ItemVariant>()
            .FirstOrDefaultAsync(iv => iv.Id == id && iv.TenantId == tenantId);

        if (entity == null)
            return false;

        _context.Set<ItemVariant>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}