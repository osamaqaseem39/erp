using ERP.Domain.Sales;

namespace ERP.Infrastructure.Interfaces.Sales;

public interface ISalesInvoiceLineRepository
{
    Task<IEnumerable<SalesInvoiceLine>> GetByInvoiceIdAsync(Guid invoiceId, Guid tenantId);
    Task<Guid> CreateAsync(SalesInvoiceLine line);
    Task<bool> DeleteByInvoiceIdAsync(Guid invoiceId, Guid tenantId);
}
