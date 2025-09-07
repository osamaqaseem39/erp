using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Interfaces.Finance;

public interface IInvoiceService
{
    // Invoices
    Task<Guid> AddInvoiceAsync(InvoiceCreateDto dto, Guid tenantId, Guid userId);
    Task<Guid> UpdateInvoiceAsync(InvoiceUpdateDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteInvoiceAsync(Guid invoiceId, Guid tenantId);
    Task<Guid> PostInvoiceAsync(Guid invoiceId, Guid tenantId, Guid postedBy);
    Task<InvoiceDto?> GetInvoiceByIdAsync(Guid invoiceId, Guid tenantId);
    Task<(IEnumerable<InvoiceDto> Invoices, int TotalCount)> SearchInvoicesAsync(InvoiceQuery query);
    
    // Invoice Lines
    Task<Guid> AddInvoiceLineAsync(InvoiceLineCreateDto dto, Guid invoiceId, Guid tenantId, Guid userId);
    Task<Guid> UpdateInvoiceLineAsync(InvoiceLineUpdateDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteInvoiceLineAsync(Guid invoiceLineId, Guid tenantId);
    Task<IEnumerable<InvoiceLineDto>> GetInvoiceLinesByInvoiceIdAsync(Guid invoiceId, Guid tenantId);
    
    // Bills (AP)
    Task<Guid> AddBillAsync(BillCreateDto dto, Guid tenantId, Guid userId);
    Task<Guid> UpdateBillAsync(BillUpdateDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteBillAsync(Guid billId, Guid tenantId);
    Task<Guid> PostBillAsync(Guid billId, Guid tenantId, Guid postedBy);
    Task<BillDto?> GetBillByIdAsync(Guid billId, Guid tenantId);
    Task<(IEnumerable<BillDto> Bills, int TotalCount)> SearchBillsAsync(BillQuery query);
    
    // Bill Lines
    Task<Guid> AddBillLineAsync(BillLineCreateDto dto, Guid billId, Guid tenantId, Guid userId);
    Task<Guid> UpdateBillLineAsync(BillLineUpdateDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteBillLineAsync(Guid billLineId, Guid tenantId);
    Task<IEnumerable<BillLineDto>> GetBillLinesByBillIdAsync(Guid billId, Guid tenantId);
    
    // Payment Application
    Task<bool> ApplyPaymentToInvoiceAsync(PaymentApplicationDto dto, Guid tenantId, Guid userId);
    Task<bool> ApplyPaymentToBillAsync(PaymentApplicationDto dto, Guid tenantId, Guid userId);
    
    // Reports
    Task<object> GetAgedReceivablesAsync(Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null);
    Task<object> GetAgedPayablesAsync(Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null);
    Task<object> GetCustomerAgingAsync(Guid customerId, Guid tenantId, DateTime asOfDate);
    Task<object> GetSupplierAgingAsync(Guid supplierId, Guid tenantId, DateTime asOfDate);
}