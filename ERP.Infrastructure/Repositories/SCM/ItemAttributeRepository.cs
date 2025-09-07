using ERP.Domain.SCM;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.SCM;

public class ItemAttributeRepository : IItemAttributeRepository
{
    private readonly ERPDbContext _context;

    public ItemAttributeRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ItemAttribute>> GetAllAsync(Guid tenantId, bool? isActive = null)
    {
        var query = _context.Set<ItemAttribute>()
            .Where(ia => ia.TenantId == tenantId);

        if (isActive.HasValue)
            query = query.Where(ia => ia.IsActive == isActive.Value);

        return await query.ToListAsync();
    }

    public async Task<ItemAttribute?> GetByIdAsync(Guid id, Guid tenantId)
    {
        return await _context.Set<ItemAttribute>()
            .FirstOrDefaultAsync(ia => ia.Id == id && ia.TenantId == tenantId);
    }

    public async Task<ItemAttribute> CreateAsync(ItemAttribute entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _context.Set<ItemAttribute>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<ItemAttribute> UpdateAsync(ItemAttribute entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Set<ItemAttribute>().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _context.Set<ItemAttribute>()
            .FirstOrDefaultAsync(ia => ia.Id == id && ia.TenantId == tenantId);

        if (entity == null)
            return false;

        _context.Set<ItemAttribute>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}