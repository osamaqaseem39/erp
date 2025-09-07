using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Finance;

public class ChartOfAccountRepository : IChartOfAccountRepository
{
    private readonly ERPDbContext _context;

    public ChartOfAccountRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<ChartOfAccount?> GetByIdAsync(Guid id)
    {
        return await _context.ChartOfAccounts
            .Include(c => c.ParentAccount)
            .Include(c => c.ChildAccounts)
            .FirstOrDefaultAsync(c => c.AccountId == id);
    }

    public async Task<IEnumerable<ChartOfAccount>> GetAllAsync()
    {
        return await _context.ChartOfAccounts
            .Include(c => c.ParentAccount)
            .Include(c => c.ChildAccounts)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChartOfAccount>> GetByTypeAsync(string accountType)
    {
        return await _context.ChartOfAccounts
            .Include(c => c.ParentAccount)
            .Include(c => c.ChildAccounts)
            .Where(c => c.AccountType == accountType)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChartOfAccount>> GetByParentAsync(Guid? parentAccountId)
    {
        return await _context.ChartOfAccounts
            .Include(c => c.ParentAccount)
            .Include(c => c.ChildAccounts)
            .Where(c => c.ParentAccountId == parentAccountId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChartOfAccount>> GetActiveAsync()
    {
        return await _context.ChartOfAccounts
            .Include(c => c.ParentAccount)
            .Include(c => c.ChildAccounts)
            .Where(c => c.IsActive)
            .ToListAsync();
    }

    public async Task<ChartOfAccount?> GetByCodeAsync(string accountCode)
    {
        return await _context.ChartOfAccounts
            .Include(c => c.ParentAccount)
            .Include(c => c.ChildAccounts)
            .FirstOrDefaultAsync(c => c.AccountCode == accountCode);
    }

    public async Task<Guid> CreateAsync(ChartOfAccount chartOfAccount)
    {
        _context.ChartOfAccounts.Add(chartOfAccount);
        await _context.SaveChangesAsync();
        return chartOfAccount.AccountId;
    }

    public async Task<bool> UpdateAsync(ChartOfAccount chartOfAccount)
    {
        _context.ChartOfAccounts.Update(chartOfAccount);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var chartOfAccount = await _context.ChartOfAccounts.FindAsync(id);
        if (chartOfAccount == null) return false;

        _context.ChartOfAccounts.Remove(chartOfAccount);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}