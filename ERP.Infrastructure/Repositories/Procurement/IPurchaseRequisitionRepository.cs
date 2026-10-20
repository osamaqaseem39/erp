using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Repositories.Procurement;

public interface IPurchaseRequisitionRepository
{
    Task<PurchaseRequisition?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PurchaseRequisition>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<PurchaseRequisition>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<PurchaseRequisition>> GetByRequestorAsync(Guid requestorId, Guid tenantId);
    Task<IEnumerable<PurchaseRequisition>> GetByDateRangeAsync(DateTime start, DateTime end, Guid tenantId);
    Task<Guid> CreateAsync(PurchaseRequisition purchaseRequisition);
    Task<bool> UpdateAsync(PurchaseRequisition purchaseRequisition);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
