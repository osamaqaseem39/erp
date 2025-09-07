using ERP.Domain.Finance;

namespace ERP.Infrastructure.Interfaces.Finance;

public interface IChartOfAccountRepository
{
    Task<ChartOfAccount?> GetByIdAsync(Guid id);
    Task<IEnumerable<ChartOfAccount>> GetAllAsync();
    Task<Guid> CreateAsync(ChartOfAccount account);
    Task<bool> UpdateAsync(ChartOfAccount account);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<ChartOfAccount?> GetByCodeAsync(string code);
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);
    Task<IEnumerable<ChartOfAccount>> GetByTypeAsync(string accountType);
    Task<IEnumerable<ChartOfAccount>> GetByParentAsync(Guid? parentAccountId);
    Task<IEnumerable<ChartOfAccount>> GetActiveAccountsAsync();
}