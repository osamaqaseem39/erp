using ERP.Domain.Finance;

namespace ERP.Infrastructure.Repositories.Finance;

public interface IBankAccountRepository
{
    Task<BankAccount?> GetByIdAsync(Guid id);
    Task<IEnumerable<BankAccount>> GetAllAsync();
    Task<IEnumerable<BankAccount>> GetByBankAsync(Guid bankId);
    Task<IEnumerable<BankAccount>> GetActiveAsync();
    Task<BankAccount?> GetByAccountNumberAsync(string accountNumber);
    Task<Guid> CreateAsync(BankAccount bankAccount);
    Task<bool> UpdateAsync(BankAccount bankAccount);
    Task<bool> DeleteAsync(Guid id);
}