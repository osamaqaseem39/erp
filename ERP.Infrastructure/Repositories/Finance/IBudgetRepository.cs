using ERP.Domain.Finance;

namespace ERP.Infrastructure.Repositories.Finance;

public interface IBudgetRepository
{
    Task<Budget?> GetByIdAsync(Guid id);
    Task<IEnumerable<Budget>> GetAllAsync();
    Task<IEnumerable<Budget>> GetByAccountAsync(Guid accountId);
    Task<IEnumerable<Budget>> GetByPeriodAsync(DateTime startDate, DateTime endDate);
    Task<Guid> CreateAsync(Budget budget);
    Task<bool> UpdateAsync(Budget budget);
    Task<bool> DeleteAsync(Guid id);
}