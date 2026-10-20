using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Interfaces.Procurement;

public interface IPurchaseInvoiceDetailRepository
{
    Task<IEnumerable<PurchaseInvoiceDetail>> GetByInvoiceIdAsync(Guid invoiceId, Guid tenantId);
    Task<Guid> CreateAsync(PurchaseInvoiceDetail detail);
    Task<bool> DeleteByInvoiceIdAsync(Guid invoiceId, Guid tenantId);
}
