using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Interfaces;

public interface IChartOfAccountsService
{
    Task<ChartOfAccountDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ChartOfAccountDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateChartOfAccountDto dto);
    Task<bool> UpdateAsync(UpdateChartOfAccountDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<ChartOfAccountDto>> GetByTypeAsync(string accountType);
    Task<IEnumerable<ChartOfAccountDto>> GetByParentAsync(Guid parentAccountId);
    Task<IEnumerable<ChartOfAccountDto>> GetActiveAccountsAsync();
}