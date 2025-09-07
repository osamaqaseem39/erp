using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Finance;

public class BudgetRepository : IBudgetRepository
{
    private readonly ERPDbContext _context;

    public BudgetRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Budget?> GetByIdAsync(Guid id)
    {
        return await _context.Budgets
            .Include(b => b.Account)
            .FirstOrDefaultAsync(b => b.BudgetId == id);
    }

    public async Task<IEnumerable<Budget>> GetAllAsync()
    {
        return await _context.Budgets
            .Include(b => b.Account)
            .ToListAsync();
    }

    public async Task<IEnumerable<Budget>> GetByAccountAsync(Guid accountId)
    {
        return await _context.Budgets
            .Include(b => b.Account)
            .Where(b => b.AccountId == accountId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Budget>> GetByPeriodAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Budgets
            .Include(b => b.Account)
            .Where(b => b.BudgetPeriodStart >= startDate && b.BudgetPeriodEnd <= endDate)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(Budget budget)
    {
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();
        return budget.BudgetId;
    }

    public async Task<bool> UpdateAsync(Budget budget)
    {
        _context.Budgets.Update(budget);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var budget = await _context.Budgets.FindAsync(id);
        if (budget == null) return false;

        _context.Budgets.Remove(budget);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}