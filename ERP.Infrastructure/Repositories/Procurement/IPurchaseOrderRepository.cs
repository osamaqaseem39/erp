using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Repositories.Procurement;

public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder?> GetByIdAsync(Guid id);
    Task<IEnumerable<PurchaseOrder>> GetAllAsync();
    Task<IEnumerable<PurchaseOrder>> GetByVendorAsync(Guid vendorId);
    Task<IEnumerable<PurchaseOrder>> GetByStatusAsync(string status);
    Task<PurchaseOrder?> GetByNumberAsync(string purchaseOrderNumber);
    Task<Guid> CreateAsync(PurchaseOrder purchaseOrder);
    Task<bool> UpdateAsync(PurchaseOrder purchaseOrder);
    Task<bool> DeleteAsync(Guid id);
}