using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class RFQVendorResponseRepository : IRFQVendorResponseRepository
{
    private readonly ERPDbContext _context;

    public RFQVendorResponseRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<RFQVendorResponse?> GetByIdAsync(int id)
    {
        return await _context.RFQVendorResponses.FindAsync(id);
    }

    public async Task<IEnumerable<RFQVendorResponse>> GetAllAsync()
    {
        return await _context.RFQVendorResponses.ToListAsync();
    }

    public async Task<RFQVendorResponse> AddAsync(RFQVendorResponse entity)
    {
        _context.RFQVendorResponses.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(RFQVendorResponse entity)
    {
        _context.RFQVendorResponses.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.RFQVendorResponses.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}