using ERP.Domain.Sales;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Sales;

public class POSTransactionRepository : IPOSTransactionRepository
{
    private readonly ERPDbContext _context;

    public POSTransactionRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<POSTransaction?> GetByIdAsync(int id)
    {
        return await _context.POSTransactions.FindAsync(id);
    }

    public async Task<IEnumerable<POSTransaction>> GetAllAsync()
    {
        return await _context.POSTransactions.ToListAsync();
    }

    public async Task<POSTransaction> AddAsync(POSTransaction entity)
    {
        _context.POSTransactions.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(POSTransaction entity)
    {
        _context.POSTransactions.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.POSTransactions.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}