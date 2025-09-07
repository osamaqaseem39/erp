using ERP.Domain.SCM;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.SCM;

public class SerialNumberRepository : ISerialNumberRepository
{
    private readonly ERPDbContext _context;

    public SerialNumberRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SerialNumber>> GetByItemAsync(Guid itemId, Guid tenantId, string? status = null)
    {
        var query = _context.Set<SerialNumber>()
            .Include(sn => sn.Item)
            .Where(sn => sn.ItemId == itemId && sn.TenantId == tenantId && sn.IsActive);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(sn => sn.Status == status);

        return await query.ToListAsync();
    }

    public async Task<SerialNumber?> GetByIdAsync(Guid id, Guid tenantId)
    {
        return await _context.Set<SerialNumber>()
            .Include(sn => sn.Item)
            .FirstOrDefaultAsync(sn => sn.Id == id && sn.TenantId == tenantId);
    }

    public async Task<SerialNumber> CreateAsync(SerialNumber entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _context.Set<SerialNumber>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<SerialNumber> UpdateAsync(SerialNumber entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Set<SerialNumber>().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _context.Set<SerialNumber>()
            .FirstOrDefaultAsync(sn => sn.Id == id && sn.TenantId == tenantId);

        if (entity == null)
            return false;

        _context.Set<SerialNumber>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}