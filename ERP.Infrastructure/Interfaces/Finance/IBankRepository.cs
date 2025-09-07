using ERP.Domain.Finance;

namespace ERP.Infrastructure.Interfaces.Finance;

public interface IBankRepository
{
    Task<Bank?> GetByIdAsync(Guid id);
    Task<IEnumerable<Bank>> GetAllAsync();
    Task<Bank> CreateAsync(Bank bank);
    Task<Bank> UpdateAsync(Bank bank);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Bank?> GetByCodeAsync(string code);
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);
    
    // Bank Account methods
    Task<IEnumerable<BankAccount>> GetBankAccountsAsync(Guid bankId);
    Task<BankAccount?> GetBankAccountByIdAsync(Guid id);
    Task<Guid> CreateBankAccountAsync(BankAccount bankAccount);
    Task<bool> UpdateBankAccountAsync(BankAccount bankAccount);
    Task<bool> DeleteBankAccountAsync(Guid id);
}