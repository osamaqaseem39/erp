using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Repositories.Procurement;

public interface IPurchaseInvoiceRepository
{
    Task<PurchaseInvoice?> GetByIdAsync(Guid id);
    Task<IEnumerable<PurchaseInvoice>> GetAllAsync();
    Task<IEnumerable<PurchaseInvoice>> GetByVendorAsync(Guid vendorId);
    Task<IEnumerable<PurchaseInvoice>> GetByStatusAsync(string status);
    Task<PurchaseInvoice?> GetByNumberAsync(string invoiceNumber);
    Task<Guid> CreateAsync(PurchaseInvoice purchaseInvoice);
    Task<bool> UpdateAsync(PurchaseInvoice purchaseInvoice);
    Task<bool> DeleteAsync(Guid id);
}