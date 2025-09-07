using ERP.Domain.Sales;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Sales;

public class POSSessionRepository : IPOSSessionRepository
{
    private readonly ERPDbContext _context;

    public POSSessionRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<POSSession?> GetByIdAsync(int id)
    {
        return await _context.POSSessions.FindAsync(id);
    }

    public async Task<IEnumerable<POSSession>> GetAllAsync()
    {
        return await _context.POSSessions.ToListAsync();
    }

    public async Task<POSSession> AddAsync(POSSession entity)
    {
        _context.POSSessions.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(POSSession entity)
    {
        _context.POSSessions.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.POSSessions.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}