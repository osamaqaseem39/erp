using ERP.Domain.Finance;

namespace ERP.Infrastructure.Interfaces.Finance;

public interface ICurrencyRepository
{
    Task<Currency?> GetByIdAsync(Guid id);
    Task<IEnumerable<Currency>> GetAllAsync();
    Task<Currency> CreateAsync(Currency currency);
    Task<Currency> UpdateAsync(Currency currency);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Currency?> GetByCodeAsync(string code);
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);
}