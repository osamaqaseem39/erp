using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Finance;

public class BankTransactionRepository : IBankTransactionRepository
{
    private readonly ERPDbContext _context;

    public BankTransactionRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<BankTransaction?> GetByIdAsync(int id)
    {
        return await _context.BankTransactions.FindAsync(id);
    }

    public async Task<IEnumerable<BankTransaction>> GetAllAsync()
    {
        return await _context.BankTransactions.ToListAsync();
    }

    public async Task<BankTransaction> AddAsync(BankTransaction entity)
    {
        _context.BankTransactions.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(BankTransaction entity)
    {
        _context.BankTransactions.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.BankTransactions.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}