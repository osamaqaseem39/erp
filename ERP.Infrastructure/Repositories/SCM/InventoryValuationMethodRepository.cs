using ERP.Domain.SCM;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.SCM;

public class InventoryValuationMethodRepository : IInventoryValuationMethodRepository
{
    private readonly ERPDbContext _context;

    public InventoryValuationMethodRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<InventoryValuationMethod>> GetAllAsync(Guid tenantId, bool? isActive = null)
    {
        var query = _context.Set<InventoryValuationMethod>()
            .Where(ivm => ivm.TenantId == tenantId);

        if (isActive.HasValue)
            query = query.Where(ivm => ivm.IsActive == isActive.Value);

        return await query.ToListAsync();
    }

    public async Task<InventoryValuationMethod?> GetByIdAsync(Guid id, Guid tenantId)
    {
        return await _context.Set<InventoryValuationMethod>()
            .FirstOrDefaultAsync(ivm => ivm.Id == id && ivm.TenantId == tenantId);
    }

    public async Task<InventoryValuationMethod> CreateAsync(InventoryValuationMethod entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _context.Set<InventoryValuationMethod>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<InventoryValuationMethod> UpdateAsync(InventoryValuationMethod entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Set<InventoryValuationMethod>().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _context.Set<InventoryValuationMethod>()
            .FirstOrDefaultAsync(ivm => ivm.Id == id && ivm.TenantId == tenantId);

        if (entity == null)
            return false;

        _context.Set<InventoryValuationMethod>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}