using ERP.Domain.Finance;

namespace ERP.Infrastructure.Repositories.Finance;

public interface ICostCenterRepository
{
    Task<CostCenter?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<CostCenter>> GetAllAsync(Guid tenantId, bool? isActive = null);
    Task<CostCenter?> GetByCodeAsync(string costCenterCode, Guid tenantId);
    Task<bool> IsReferencedByJournalEntriesAsync(Guid costCenterId, Guid tenantId);
    Task<Guid> CreateAsync(CostCenter costCenter);
    Task<bool> UpdateAsync(CostCenter costCenter);
    Task<bool> SoftDeactivateAsync(Guid id, Guid tenantId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
