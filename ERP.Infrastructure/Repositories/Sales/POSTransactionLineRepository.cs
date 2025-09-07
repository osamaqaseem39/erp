using ERP.Domain.Sales;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Sales;

public class POSTransactionLineRepository : IPOSTransactionLineRepository
{
    private readonly ERPDbContext _context;

    public POSTransactionLineRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<POSTransactionLine?> GetByIdAsync(int id)
    {
        return await _context.POSTransactionLines.FindAsync(id);
    }

    public async Task<IEnumerable<POSTransactionLine>> GetAllAsync()
    {
        return await _context.POSTransactionLines.ToListAsync();
    }

    public async Task<POSTransactionLine> AddAsync(POSTransactionLine entity)
    {
        _context.POSTransactionLines.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(POSTransactionLine entity)
    {
        _context.POSTransactionLines.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.POSTransactionLines.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}