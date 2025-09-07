using ERP.Domain.SCM;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.SCM;

public class ItemGroupRepository : IItemGroupRepository
{
    private readonly ERPDbContext _context;

    public ItemGroupRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ItemGroup>> GetAllAsync(Guid tenantId, bool? isActive = null)
    {
        var query = _context.Set<ItemGroup>()
            .Include(ig => ig.ParentGroup)
            .Where(ig => ig.TenantId == tenantId);

        if (isActive.HasValue)
            query = query.Where(ig => ig.IsActive == isActive.Value);

        return await query.ToListAsync();
    }

    public async Task<ItemGroup?> GetByIdAsync(Guid id, Guid tenantId)
    {
        return await _context.Set<ItemGroup>()
            .Include(ig => ig.ParentGroup)
            .FirstOrDefaultAsync(ig => ig.Id == id && ig.TenantId == tenantId);
    }

    public async Task<ItemGroup> CreateAsync(ItemGroup entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _context.Set<ItemGroup>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<ItemGroup> UpdateAsync(ItemGroup entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Set<ItemGroup>().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _context.Set<ItemGroup>()
            .FirstOrDefaultAsync(ig => ig.Id == id && ig.TenantId == tenantId);

        if (entity == null)
            return false;

        _context.Set<ItemGroup>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}