using ERP.Domain.Sales;

namespace ERP.Infrastructure.Interfaces.Sales;

public interface ISalesInvoiceRepository
{
    Task<SalesInvoice?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<SalesInvoice>> GetAllAsync(Guid tenantId);
    Task<SalesInvoice> AddAsync(SalesInvoice entity);
    Task UpdateAsync(SalesInvoice entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<SalesInvoice>> GetByCustomerAsync(Guid customerId, Guid tenantId);
    Task<IEnumerable<SalesInvoice>> GetByStatusAsync(string status, Guid tenantId);
    Task<bool> PostInvoiceAsync(Guid invoiceId, Guid tenantId, Guid postedBy);
    Task<bool> VoidInvoiceAsync(Guid invoiceId, Guid tenantId, Guid voidedBy);
}