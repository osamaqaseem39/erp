using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Finance;

public class DepreciationEntryRepository : IDepreciationEntryRepository
{
    private readonly ERPDbContext _context;

    public DepreciationEntryRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<DepreciationEntry?> GetByIdAsync(int id)
    {
        return await _context.DepreciationEntries.FindAsync(id);
    }

    public async Task<IEnumerable<DepreciationEntry>> GetAllAsync()
    {
        return await _context.DepreciationEntries.ToListAsync();
    }

    public async Task<DepreciationEntry> AddAsync(DepreciationEntry entity)
    {
        _context.DepreciationEntries.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(DepreciationEntry entity)
    {
        _context.DepreciationEntries.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.DepreciationEntries.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}