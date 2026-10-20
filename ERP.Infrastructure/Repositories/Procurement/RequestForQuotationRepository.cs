using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class RequestForQuotationRepository : IRequestForQuotationRepository
{
    private readonly ERPDbContext _context;

    public RequestForQuotationRepository(ERPDbContext context) => _context = context;

    public async Task<RequestForQuotation?> GetByIdAsync(Guid id, Guid tenantId)
        => await _context.RequestForQuotations
            .Include(rfq => rfq.Details).ThenInclude(d => d.Item)
            .Include(rfq => rfq.VendorResponses)
            .FirstOrDefaultAsync(rfq => rfq.Id == id && rfq.TenantId == tenantId);

    public async Task<IEnumerable<RequestForQuotation>> GetAllAsync(Guid tenantId)
        => await _context.RequestForQuotations.AsNoTracking()
            .Include(rfq => rfq.Details)
            .Where(rfq => rfq.TenantId == tenantId)
            .OrderByDescending(rfq => rfq.IssuedDate)
            .ToListAsync();

    public async Task<IEnumerable<RequestForQuotation>> GetByStatusAsync(string status, Guid tenantId)
        => await _context.RequestForQuotations.AsNoTracking()
            .Include(rfq => rfq.Details)
            .Where(rfq => rfq.TenantId == tenantId && rfq.Status == status)
            .ToListAsync();

    public async Task<IEnumerable<RequestForQuotation>> GetByRequestorAsync(Guid requestorId, Guid tenantId)
        => await _context.RequestForQuotations.AsNoTracking()
            .Include(rfq => rfq.Details)
            .Where(rfq => rfq.TenantId == tenantId && rfq.RequestorId == requestorId)
            .ToListAsync();

    public async Task<Guid> CreateAsync(RequestForQuotation requestForQuotation)
    {
        _context.RequestForQuotations.Add(requestForQuotation);
        await _context.SaveChangesAsync();
        return requestForQuotation.Id;
    }

    public async Task<bool> UpdateAsync(RequestForQuotation requestForQuotation)
    {
        _context.RequestForQuotations.Update(requestForQuotation);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _context.RequestForQuotations
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);
        if (entity == null) return false;
        _context.RFQDetails.RemoveRange(entity.Details);
        _context.RequestForQuotations.Remove(entity);
        return await _context.SaveChangesAsync() > 0;
    }
}
