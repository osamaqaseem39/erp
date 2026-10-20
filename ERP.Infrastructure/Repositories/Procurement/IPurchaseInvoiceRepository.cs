using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Repositories.Procurement;

public interface IPurchaseInvoiceRepository
{
    Task<PurchaseInvoice?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PurchaseInvoice>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<PurchaseInvoice>> GetByVendorAsync(Guid vendorId, Guid tenantId);
    Task<IEnumerable<PurchaseInvoice>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<PurchaseInvoice>> GetByDateRangeAsync(DateTime start, DateTime end, Guid tenantId);
    Task<Guid> CreateAsync(PurchaseInvoice purchaseInvoice);
    Task<bool> UpdateAsync(PurchaseInvoice purchaseInvoice);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
