using ERP.Domain.Finance;

namespace ERP.Infrastructure.Interfaces.Finance;

public interface IDepreciationEntryRepository
{
    Task<DepreciationEntry?> GetByIdAsync(int id);
    Task<IEnumerable<DepreciationEntry>> GetAllAsync();
    Task<DepreciationEntry> AddAsync(DepreciationEntry entity);
    Task UpdateAsync(DepreciationEntry entity);
    Task DeleteAsync(int id);
}