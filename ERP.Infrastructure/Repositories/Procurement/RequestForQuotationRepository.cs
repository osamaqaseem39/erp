using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class RequestForQuotationRepository : IRequestForQuotationRepository
{
    private readonly ERPDbContext _context;

    public RequestForQuotationRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<RequestForQuotation?> GetByIdAsync(Guid id)
    {
        return await _context.RequestForQuotations
            .Include(rfq => rfq.Details)
            .Include(rfq => rfq.VendorResponses)
            .FirstOrDefaultAsync(rfq => rfq.Id == id);
    }

    public async Task<IEnumerable<RequestForQuotation>> GetAllAsync()
    {
        return await _context.RequestForQuotations
            .Include(rfq => rfq.Details)
            .Include(rfq => rfq.VendorResponses)
            .ToListAsync();
    }

    public async Task<IEnumerable<RequestForQuotation>> GetByStatusAsync(string status)
    {
        return await _context.RequestForQuotations
            .Include(rfq => rfq.Details)
            .Include(rfq => rfq.VendorResponses)
            .Where(rfq => rfq.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<RequestForQuotation>> GetByRequestorAsync(Guid requestorId)
    {
        return await _context.RequestForQuotations
            .Include(rfq => rfq.Details)
            .Include(rfq => rfq.VendorResponses)
            .Where(rfq => rfq.RequestorId == requestorId)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(RequestForQuotation requestForQuotation)
    {
        _context.RequestForQuotations.Add(requestForQuotation);
        await _context.SaveChangesAsync();
        return requestForQuotation.Id;
    }

    public async Task<bool> UpdateAsync(RequestForQuotation requestForQuotation)
    {
        _context.RequestForQuotations.Update(requestForQuotation);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var requestForQuotation = await _context.RequestForQuotations.FindAsync(id);
        if (requestForQuotation == null) return false;

        _context.RequestForQuotations.Remove(requestForQuotation);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}