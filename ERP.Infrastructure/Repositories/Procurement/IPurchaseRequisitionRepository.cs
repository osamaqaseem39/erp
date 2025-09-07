using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Repositories.Procurement;

public interface IPurchaseRequisitionRepository
{
    Task<PurchaseRequisition?> GetByIdAsync(Guid id);
    Task<IEnumerable<PurchaseRequisition>> GetAllAsync();
    Task<IEnumerable<PurchaseRequisition>> GetByStatusAsync(string status);
    Task<IEnumerable<PurchaseRequisition>> GetByRequestorAsync(Guid requestorId);
    Task<Guid> CreateAsync(PurchaseRequisition purchaseRequisition);
    Task<bool> UpdateAsync(PurchaseRequisition purchaseRequisition);
    Task<bool> DeleteAsync(Guid id);
}