using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Finance;

public class IntercompanyTransactionRepository : IIntercompanyTransactionRepository
{
    private readonly ERPDbContext _context;

    public IntercompanyTransactionRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<IntercompanyTransaction?> GetByIdAsync(int id)
    {
        return await _context.IntercompanyTransactions.FindAsync(id);
    }

    public async Task<IEnumerable<IntercompanyTransaction>> GetAllAsync()
    {
        return await _context.IntercompanyTransactions.ToListAsync();
    }

    public async Task<IntercompanyTransaction> AddAsync(IntercompanyTransaction entity)
    {
        _context.IntercompanyTransactions.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(IntercompanyTransaction entity)
    {
        _context.IntercompanyTransactions.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.IntercompanyTransactions.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}