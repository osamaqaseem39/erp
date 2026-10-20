using ERP.Domain.Finance;

namespace ERP.Infrastructure.Repositories.Finance;

public interface IBankRepository
{
    Task<Bank?> GetByIdAsync(Guid id);
    Task<Bank?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Bank>> GetAllAsync();
    Task<IEnumerable<Bank>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<Bank>> GetActiveAsync();
    Task<IEnumerable<Bank>> GetActiveAsync(Guid tenantId);
    Task<Bank?> GetByCodeAsync(string bankCode);
    Task<Bank?> GetByCodeAsync(string bankCode, Guid tenantId);
    Task<Guid> CreateAsync(Bank bank);
    Task<bool> UpdateAsync(Bank bank);
    Task<bool> DeleteAsync(Guid id);
}
