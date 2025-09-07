using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Finance;

public class BillLineRepository : IBillLineRepository
{
    private readonly ERPDbContext _context;

    public BillLineRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<BillLine?> GetByIdAsync(int id)
    {
        return await _context.BillLines.FindAsync(id);
    }

    public async Task<IEnumerable<BillLine>> GetAllAsync()
    {
        return await _context.BillLines.ToListAsync();
    }

    public async Task<BillLine> AddAsync(BillLine entity)
    {
        _context.BillLines.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(BillLine entity)
    {
        _context.BillLines.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.BillLines.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}