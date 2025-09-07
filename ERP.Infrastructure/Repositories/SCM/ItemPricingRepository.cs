using ERP.Domain.SCM;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.SCM;

public class ItemPricingRepository : IItemPricingRepository
{
    private readonly ERPDbContext _context;

    public ItemPricingRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ItemPricing>> GetByItemAsync(Guid itemId, Guid tenantId, string? priceListName = null)
    {
        var query = _context.Set<ItemPricing>()
            .Include(ip => ip.Item)
            .Where(ip => ip.ItemId == itemId && ip.TenantId == tenantId && ip.IsActive);

        if (!string.IsNullOrEmpty(priceListName))
            query = query.Where(ip => ip.PriceListName == priceListName);

        return await query.OrderByDescending(ip => ip.ValidFrom).ToListAsync();
    }

    public async Task<ItemPricing?> GetByIdAsync(Guid id, Guid tenantId)
    {
        return await _context.Set<ItemPricing>()
            .Include(ip => ip.Item)
            .FirstOrDefaultAsync(ip => ip.Id == id && ip.TenantId == tenantId);
    }

    public async Task<ItemPricing> CreateAsync(ItemPricing entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _context.Set<ItemPricing>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<ItemPricing> UpdateAsync(ItemPricing entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Set<ItemPricing>().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _context.Set<ItemPricing>()
            .FirstOrDefaultAsync(ip => ip.Id == id && ip.TenantId == tenantId);

        if (entity == null)
            return false;

        _context.Set<ItemPricing>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}