using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Procurement;

public class RFQDetailRepository : IRFQDetailRepository
{
    private readonly ERPDbContext _context;

    public RFQDetailRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<RFQDetail?> GetByIdAsync(int id)
    {
        return await _context.RFQDetails.FindAsync(id);
    }

    public async Task<IEnumerable<RFQDetail>> GetAllAsync()
    {
        return await _context.RFQDetails.ToListAsync();
    }

    public async Task<RFQDetail> AddAsync(RFQDetail entity)
    {
        _context.RFQDetails.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(RFQDetail entity)
    {
        _context.RFQDetails.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.RFQDetails.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}