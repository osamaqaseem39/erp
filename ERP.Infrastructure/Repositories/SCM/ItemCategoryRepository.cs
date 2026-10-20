using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.SCM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.SCM;

public class ItemCategoryRepository : IItemCategoryRepository
{
    private readonly ERPDbContext _context;

    public ItemCategoryRepository(ERPDbContext context) => _context = context;

    public async Task<ItemCategory?> GetByIdAsync(Guid id, Guid tenantId)
        => await _context.ItemCategories.AsNoTracking()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);

    public async Task<IEnumerable<ItemCategory>> GetAllAsync(Guid tenantId)
        => await _context.ItemCategories.AsNoTracking()
            .Include(c => c.ParentCategory)
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.CategoryCode)
            .ToListAsync();

    public async Task<IEnumerable<ItemCategory>> GetActiveAsync(Guid tenantId)
        => await _context.ItemCategories.AsNoTracking()
            .Include(c => c.ParentCategory)
            .Where(c => c.TenantId == tenantId && c.IsActive)
            .OrderBy(c => c.CategoryCode)
            .ToListAsync();

    public async Task<IEnumerable<ItemCategory>> GetByParentAsync(Guid tenantId, Guid? parentId)
        => await _context.ItemCategories.AsNoTracking()
            .Include(c => c.ParentCategory)
            .Where(c => c.TenantId == tenantId && c.ParentCategoryId == parentId)
            .OrderBy(c => c.CategoryCode)
            .ToListAsync();

    public Task<bool> CodeExistsAsync(Guid tenantId, string code, Guid? excludeId = null)
    {
        var q = _context.ItemCategories.AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.CategoryCode == code);
        if (excludeId.HasValue) q = q.Where(c => c.Id != excludeId.Value);
        return q.AnyAsync();
    }

    public async Task<ItemCategory> AddAsync(ItemCategory entity)
    {
        _context.ItemCategories.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(ItemCategory entity)
    {
        _context.ItemCategories.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _context.ItemCategories
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
        if (entity == null) return false;
        if (await _context.ItemCategories.AnyAsync(c => c.ParentCategoryId == id && c.TenantId == tenantId))
            return false;
        if (await _context.Items.AnyAsync(i => i.CategoryId == id && i.TenantId == tenantId))
        {
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        _context.ItemCategories.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
