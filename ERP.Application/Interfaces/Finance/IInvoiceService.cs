using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Interfaces.Finance;

public interface IInvoiceService
{
    // Invoices
    Task<Guid> AddInvoiceAsync(InvoiceCreateDto dto, Guid tenantId, Guid userId);
    Task<Guid> UpdateInvoiceAsync(InvoiceUpdateDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteInvoiceAsync(Guid invoiceId, Guid tenantId);
    Task<Guid> PostInvoiceAsync(Guid invoiceId, Guid tenantId, Guid postedBy);
    Task<bool> VoidInvoiceAsync(Guid invoiceId, VoidInvoiceDto dto, Guid tenantId, Guid userId);
    /// <summary>
    /// Queue customer delivery for an AR invoice (email via notifier until SMTP is wired).
    /// Promotes Draft → Issued. Requires a customer email on file.
    /// </summary>
    Task<bool> SendInvoiceAsync(Guid invoiceId, Guid tenantId, Guid userId, CancellationToken ct = default);
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

    /// <summary>Reduce AR balance (credit note / credit memo). Same mechanics as payment apply.</summary>
    Task<bool> ApplyCreditToInvoiceAsync(Guid invoiceId, decimal amount, Guid tenantId);
    /// <summary>Restore AR balance when a credit note is voided.</summary>
    Task<bool> UnapplyCreditFromInvoiceAsync(Guid invoiceId, decimal amount, Guid tenantId);
    
    // Reports
    Task<AgingReportDto> GetAgedReceivablesAsync(Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null);
    Task<AgingReportDto> GetAgedPayablesAsync(Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null);
    Task<AgingReportDto> GetCustomerAgingAsync(Guid customerId, Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null);
    Task<AgingReportDto> GetSupplierAgingAsync(Guid supplierId, Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null);
    Task<PartnerBalanceSummaryReportDto> GetPartnerBalanceSummaryAsync(Guid tenantId, DateTime asOfDate);
}