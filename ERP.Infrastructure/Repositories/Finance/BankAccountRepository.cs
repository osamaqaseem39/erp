using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Finance;

public class BankAccountRepository : IBankAccountRepository
{
    private readonly ERPDbContext _context;

    public BankAccountRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<BankAccount?> GetByIdAsync(Guid id)
    {
        return await _context.BankAccounts
            .Include(ba => ba.Bank)
            .FirstOrDefaultAsync(ba => ba.Id == id);
    }

    public async Task<IEnumerable<BankAccount>> GetAllAsync()
    {
        return await _context.BankAccounts
            .Include(ba => ba.Bank)
            .ToListAsync();
    }

    public async Task<IEnumerable<BankAccount>> GetByBankAsync(Guid bankId)
    {
        return await _context.BankAccounts
            .Include(ba => ba.Bank)
            .Where(ba => ba.BankId == bankId)
            .ToListAsync();
    }

    public async Task<IEnumerable<BankAccount>> GetActiveAsync()
    {
        return await _context.BankAccounts
            .Include(ba => ba.Bank)
            .Where(ba => ba.IsActive)
            .ToListAsync();
    }

    public async Task<BankAccount?> GetByAccountNumberAsync(string accountNumber)
    {
        return await _context.BankAccounts
            .Include(ba => ba.Bank)
            .FirstOrDefaultAsync(ba => ba.AccountNumber == accountNumber);
    }

    public async Task<Guid> CreateAsync(BankAccount bankAccount)
    {
        _context.BankAccounts.Add(bankAccount);
        await _context.SaveChangesAsync();
        return bankAccount.Id;
    }

    public async Task<bool> UpdateAsync(BankAccount bankAccount)
    {
        _context.BankAccounts.Update(bankAccount);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var bankAccount = await _context.BankAccounts.FindAsync(id);
        if (bankAccount == null) return false;

        _context.BankAccounts.Remove(bankAccount);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    // Additional methods for IBankRepository interface
    public async Task<IEnumerable<BankAccount>> GetBankAccountsAsync(Guid bankId)
        => await GetByBankAsync(bankId);

    public async Task<BankAccount?> GetBankAccountByIdAsync(Guid id)
    {
        return await GetByIdAsync(id);
    }

    public async Task<Guid> CreateBankAccountAsync(BankAccount bankAccount)
    {
        return await CreateAsync(bankAccount);
    }

    public async Task<bool> UpdateBankAccountAsync(BankAccount bankAccount)
    {
        return await UpdateAsync(bankAccount);
    }

    public async Task<bool> DeleteBankAccountAsync(Guid id)
    {
        return await DeleteAsync(id);
    }
}