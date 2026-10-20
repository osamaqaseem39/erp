using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class RFQVendorResponseRepository : IRFQVendorResponseRepository
{
    private readonly ERPDbContext _context;

    public RFQVendorResponseRepository(ERPDbContext context) => _context = context;

    private IQueryable<RFQVendorResponse> Query(Guid tenantId) =>
        _context.RFQVendorResponses.Where(r => r.TenantId == tenantId);

    public async Task<RFQVendorResponse?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await Query(tenantId).FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<RFQVendorResponse>> GetByRfqIdAsync(Guid rfqId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<RFQVendorResponse>();
        return await Query(tenantId).Where(r => r.RFQId == rfqId).ToListAsync();
    }

    public async Task<Guid> CreateAsync(RFQVendorResponse entity)
    {
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        _context.RFQVendorResponses.Add(entity);
        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(RFQVendorResponse entity)
    {
        _context.RFQVendorResponses.Update(entity);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        _context.RFQVendorResponses.Remove(entity);
        return await _context.SaveChangesAsync() > 0;
    }
}
