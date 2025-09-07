using ERP.Domain.Finance;

namespace ERP.Infrastructure.Repositories.Finance;

public interface IBankRepository
{
    Task<Bank?> GetByIdAsync(Guid id);
    Task<IEnumerable<Bank>> GetAllAsync();
    Task<IEnumerable<Bank>> GetActiveAsync();
    Task<Bank?> GetByCodeAsync(string bankCode);
    Task<Guid> CreateAsync(Bank bank);
    Task<bool> UpdateAsync(Bank bank);
    Task<bool> DeleteAsync(Guid id);
}