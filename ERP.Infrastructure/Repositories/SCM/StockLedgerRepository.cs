using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.SCM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.SCM;

public class StockLedgerRepository : IStockLedgerRepository
{
    private readonly ERPDbContext _context;

    public StockLedgerRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<StockLedger?> GetByIdAsync(int id)
    {
        return await _context.StockLedgers.FindAsync(id);
    }

    public async Task<IEnumerable<StockLedger>> GetAllAsync()
    {
        return await _context.StockLedgers.ToListAsync();
    }

    public async Task<StockLedger> AddAsync(StockLedger entity)
    {
        _context.StockLedgers.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(StockLedger entity)
    {
        _context.StockLedgers.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.StockLedgers.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}