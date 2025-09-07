using ERP.Domain.Finance;

namespace ERP.Infrastructure.Repositories.Finance;

public interface IChartOfAccountRepository
{
    Task<ChartOfAccount?> GetByIdAsync(Guid id);
    Task<IEnumerable<ChartOfAccount>> GetAllAsync();
    Task<IEnumerable<ChartOfAccount>> GetByTypeAsync(string accountType);
    Task<IEnumerable<ChartOfAccount>> GetByParentAsync(Guid? parentAccountId);
    Task<IEnumerable<ChartOfAccount>> GetActiveAsync();
    Task<ChartOfAccount?> GetByCodeAsync(string accountCode);
    Task<Guid> CreateAsync(ChartOfAccount chartOfAccount);
    Task<bool> UpdateAsync(ChartOfAccount chartOfAccount);
    Task<bool> DeleteAsync(Guid id);
}