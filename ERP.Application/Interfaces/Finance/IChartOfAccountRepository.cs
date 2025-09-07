using ERP.Domain.Finance;

namespace ERP.Application.Interfaces.Finance;

public interface IChartOfAccountRepository
{
    Task<ChartOfAccount?> GetByIdAsync(Guid accountId, Guid tenantId);
    Task<IEnumerable<ChartOfAccount>> GetAllAsync(Guid tenantId, string? accountType = null, bool? isActive = null);
    Task<ChartOfAccount?> GetByAccountCodeAsync(string accountCode, Guid tenantId);
    Task<IEnumerable<ChartOfAccount>> GetChildAccountsAsync(Guid parentAccountId, Guid tenantId);
    Task<ChartOfAccount> AddAsync(ChartOfAccount account);
    Task<ChartOfAccount> UpdateAsync(ChartOfAccount account);
    Task<bool> DeleteAsync(Guid accountId, Guid tenantId);
    Task<bool> HasChildAccountsAsync(Guid accountId, Guid tenantId);
    Task<bool> HasJournalEntriesAsync(Guid accountId, Guid tenantId);
    Task<IEnumerable<ChartOfAccount>> GetByAccountTypeAsync(string accountType, Guid tenantId);
    Task<IEnumerable<ChartOfAccount>> GetPostingAccountsAsync(Guid tenantId);
}