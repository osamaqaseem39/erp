using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Finance;

public class BankReconciliationRepository : IBankReconciliationRepository
{
    private readonly ERPDbContext _context;

    public BankReconciliationRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<BankReconciliation?> GetByIdAsync(int id)
    {
        return await _context.BankReconciliations.FindAsync(id);
    }

    public async Task<IEnumerable<BankReconciliation>> GetAllAsync()
    {
        return await _context.BankReconciliations.ToListAsync();
    }

    public async Task<BankReconciliation> AddAsync(BankReconciliation entity)
    {
        _context.BankReconciliations.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(BankReconciliation entity)
    {
        _context.BankReconciliations.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.BankReconciliations.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}