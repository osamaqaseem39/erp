using ERP.Domain.Finance;

namespace ERP.Infrastructure.Repositories.Finance;

public interface ICostCenterRepository
{
    Task<CostCenter?> GetByIdAsync(Guid id);
    Task<IEnumerable<CostCenter>> GetAllAsync();
    Task<IEnumerable<CostCenter>> GetActiveAsync();
    Task<CostCenter?> GetByCodeAsync(string costCenterCode);
    Task<Guid> CreateAsync(CostCenter costCenter);
    Task<bool> UpdateAsync(CostCenter costCenter);
    Task<bool> DeleteAsync(Guid id);
}