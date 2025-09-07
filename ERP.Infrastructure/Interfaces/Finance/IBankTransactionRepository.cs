using ERP.Domain.Finance;

namespace ERP.Infrastructure.Interfaces.Finance;

public interface IBankTransactionRepository
{
    Task<BankTransaction?> GetByIdAsync(int id);
    Task<IEnumerable<BankTransaction>> GetAllAsync();
    Task<BankTransaction> AddAsync(BankTransaction entity);
    Task UpdateAsync(BankTransaction entity);
    Task DeleteAsync(int id);
}