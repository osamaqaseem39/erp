using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Interfaces.Procurement;

public interface IPurchaseOrderDetailRepository
{
    Task<IEnumerable<PurchaseOrderDetail>> GetByPurchaseOrderIdAsync(Guid poId, Guid tenantId);
    Task<Guid> CreateAsync(PurchaseOrderDetail detail);
    Task<bool> UpdateAsync(PurchaseOrderDetail detail);
    Task<bool> DeleteByPurchaseOrderIdAsync(Guid poId, Guid tenantId);
}
