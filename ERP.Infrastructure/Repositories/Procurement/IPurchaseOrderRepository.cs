using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Repositories.Procurement;

public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PurchaseOrder>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<PurchaseOrder>> GetByVendorAsync(Guid vendorId, Guid tenantId);
    Task<IEnumerable<PurchaseOrder>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<PurchaseOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
    Task<PurchaseOrder?> GetByNumberAsync(string purchaseOrderNumber, Guid tenantId);
    Task<Guid> CreateAsync(PurchaseOrder purchaseOrder);
    Task<bool> UpdateAsync(PurchaseOrder purchaseOrder);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
