using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Finance;

public class BankRepository : IBankRepository
{
    private readonly ERPDbContext _context;

    public BankRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Bank?> GetByIdAsync(Guid id)
    {
        return await _context.Banks
            .Include(b => b.BankAccounts)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Bank?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await _context.Banks
            .Include(b => b.BankAccounts)
            .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);
    }

    public async Task<IEnumerable<Bank>> GetAllAsync()
    {
        return await _context.Banks
            .Include(b => b.BankAccounts)
            .ToListAsync();
    }

    public async Task<IEnumerable<Bank>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<Bank>();
        return await _context.Banks
            .Include(b => b.BankAccounts)
            .Where(b => b.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Bank>> GetActiveAsync()
    {
        return await _context.Banks
            .Include(b => b.BankAccounts)
            .Where(b => b.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Bank>> GetActiveAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<Bank>();
        return await _context.Banks
            .Include(b => b.BankAccounts)
            .Where(b => b.IsActive && b.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<Bank?> GetByCodeAsync(string bankCode)
    {
        return await _context.Banks
            .Include(b => b.BankAccounts)
            .FirstOrDefaultAsync(b => b.BankCode == bankCode);
    }

    public async Task<Bank?> GetByCodeAsync(string bankCode, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await _context.Banks
            .Include(b => b.BankAccounts)
            .FirstOrDefaultAsync(b => b.BankCode == bankCode && b.TenantId == tenantId);
    }

    public async Task<Guid> CreateAsync(Bank bank)
    {
        _context.Banks.Add(bank);
        await _context.SaveChangesAsync();
        return bank.Id;
    }

    public async Task<bool> UpdateAsync(Bank bank)
    {
        _context.Banks.Update(bank);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var bank = await _context.Banks.FindAsync(id);
        if (bank == null) return false;

        _context.Banks.Remove(bank);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}
