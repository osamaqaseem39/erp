using ERP.Domain.SCM;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.SCM;

public class BarcodeRepository : IBarcodeRepository
{
    private readonly ERPDbContext _context;

    public BarcodeRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Barcode>> GetByItemAsync(Guid? itemId, Guid? variantId, Guid tenantId)
    {
        var query = _context.Set<Barcode>()
            .Include(b => b.Item)
            .Include(b => b.ItemVariant)
            .Where(b => b.TenantId == tenantId && b.IsActive);

        if (itemId.HasValue)
            query = query.Where(b => b.ItemId == itemId.Value);

        if (variantId.HasValue)
            query = query.Where(b => b.VariantId == variantId.Value);

        return await query.ToListAsync();
    }

    public async Task<Barcode?> GetByIdAsync(Guid id, Guid tenantId)
    {
        return await _context.Set<Barcode>()
            .Include(b => b.Item)
            .Include(b => b.ItemVariant)
            .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);
    }

    public async Task<Barcode> CreateAsync(Barcode entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _context.Set<Barcode>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Barcode> UpdateAsync(Barcode entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Set<Barcode>().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _context.Set<Barcode>()
            .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);

        if (entity == null)
            return false;

        _context.Set<Barcode>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}