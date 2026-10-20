using ERP.Domain.SCM;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.SCM;

public class UnitOfMeasureRepository : IUnitOfMeasureRepository
{
    private readonly ERPDbContext _context;

    public UnitOfMeasureRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UnitOfMeasure>> GetAllAsync(Guid tenantId, bool? isActive = null)
    {
        if (tenantId == Guid.Empty)
            return Array.Empty<UnitOfMeasure>();

        var query = _context.Set<UnitOfMeasure>()
            .Include(uom => uom.BaseUnit)
            .Where(uom => uom.TenantId == tenantId);

        if (isActive.HasValue)
            query = query.Where(uom => uom.IsActive == isActive.Value);

        return await query.OrderBy(uom => uom.UnitName).ToListAsync();
    }

    public async Task<UnitOfMeasure?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return null;

        return await _context.Set<UnitOfMeasure>()
            .Include(uom => uom.BaseUnit)
            .FirstOrDefaultAsync(uom => uom.Id == id && uom.TenantId == tenantId);
    }

    public async Task<UnitOfMeasure> CreateAsync(UnitOfMeasure entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _context.Set<UnitOfMeasure>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<UnitOfMeasure> UpdateAsync(UnitOfMeasure entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Set<UnitOfMeasure>().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return false;

        var entity = await _context.Set<UnitOfMeasure>()
            .FirstOrDefaultAsync(uom => uom.Id == id && uom.TenantId == tenantId);

        if (entity == null)
            return false;

        _context.Set<UnitOfMeasure>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
