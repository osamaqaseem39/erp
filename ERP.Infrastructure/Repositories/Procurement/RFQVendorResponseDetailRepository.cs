using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class RFQVendorResponseDetailRepository : IRFQVendorResponseDetailRepository
{
    private readonly ERPDbContext _context;

    public RFQVendorResponseDetailRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<RFQVendorResponseDetail?> GetByIdAsync(int id)
    {
        return await _context.RFQVendorResponseDetails.FindAsync(id);
    }

    public async Task<IEnumerable<RFQVendorResponseDetail>> GetAllAsync()
    {
        return await _context.RFQVendorResponseDetails.ToListAsync();
    }

    public async Task<RFQVendorResponseDetail> AddAsync(RFQVendorResponseDetail entity)
    {
        _context.RFQVendorResponseDetails.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(RFQVendorResponseDetail entity)
    {
        _context.RFQVendorResponseDetails.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.RFQVendorResponseDetails.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}