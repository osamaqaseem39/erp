using ERP.Shared.DTOs.Sales;

namespace ERP.Application.Interfaces.Sales;

public interface ISalesInvoiceService
{
    Task<SalesInvoiceDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<SalesInvoiceDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateSalesInvoiceDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateSalesInvoiceDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<SalesInvoiceDto>> GetByCustomerAsync(Guid customerId, Guid tenantId);
    Task<IEnumerable<SalesInvoiceDto>> GetByStatusAsync(string status, Guid tenantId);
    Task<bool> PostInvoiceAsync(Guid invoiceId, Guid tenantId, Guid postedBy);
    Task<bool> VoidInvoiceAsync(Guid invoiceId, Guid tenantId, Guid voidedBy);
}

