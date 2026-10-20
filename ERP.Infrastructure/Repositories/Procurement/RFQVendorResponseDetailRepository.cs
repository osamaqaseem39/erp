using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class RFQVendorResponseDetailRepository : IRFQVendorResponseDetailRepository
{
    private readonly ERPDbContext _context;

    public RFQVendorResponseDetailRepository(ERPDbContext context) => _context = context;

    private IQueryable<RFQVendorResponseDetail> Query(Guid tenantId) =>
        _context.RFQVendorResponseDetails.Where(d => d.TenantId == tenantId);

    public async Task<RFQVendorResponseDetail?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await Query(tenantId).FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<RFQVendorResponseDetail>> GetByResponseAsync(Guid responseId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<RFQVendorResponseDetail>();
        return await Query(tenantId).Where(d => d.RFQResponseId == responseId).ToListAsync();
    }

    public async Task<Guid> CreateAsync(RFQVendorResponseDetail entity)
    {
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        _context.RFQVendorResponseDetails.Add(entity);
        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(RFQVendorResponseDetail entity)
    {
        _context.RFQVendorResponseDetails.Update(entity);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        _context.RFQVendorResponseDetails.Remove(entity);
        return await _context.SaveChangesAsync() > 0;
    }
}
