using ERP.Domain.Manufacturing;

namespace ERP.Infrastructure.Repositories.Manufacturing;

public interface IBillOfMaterialRepository
{
    Task<BillOfMaterial?> GetByIdAsync(Guid id, Guid tenantId);
    Task<BillOfMaterial?> GetActiveByFinishedGoodAsync(Guid finishedGoodItemId, Guid tenantId);
    Task<IEnumerable<BillOfMaterial>> GetAllAsync(Guid tenantId, Guid? finishedGoodItemId = null);
    Task<Guid> CreateAsync(BillOfMaterial entity);
    Task<bool> UpdateAsync(BillOfMaterial entity);
    Task<bool> SoftDeleteAsync(Guid id, Guid tenantId);
}
