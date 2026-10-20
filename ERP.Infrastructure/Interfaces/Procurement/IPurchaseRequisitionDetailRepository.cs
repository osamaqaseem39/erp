using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Interfaces.Procurement;

public interface IPurchaseRequisitionDetailRepository
{
    Task<IEnumerable<PurchaseRequisitionDetail>> GetByRequisitionIdAsync(Guid prId, Guid tenantId);
    Task<Guid> CreateAsync(PurchaseRequisitionDetail detail);
    Task<bool> DeleteByRequisitionIdAsync(Guid prId, Guid tenantId);
}
