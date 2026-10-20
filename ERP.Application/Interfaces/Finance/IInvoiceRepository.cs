using ERP.Domain.Finance;

namespace ERP.Application.Interfaces.Finance;

public interface IInvoiceRepository
{
    // Invoices
    Task<Invoice?> GetByIdAsync(Guid invoiceId, Guid tenantId);
    Task<IEnumerable<Invoice>> GetAllAsync(Guid tenantId, Guid? customerId = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 50);
    Task<int> GetCountAsync(Guid tenantId, Guid? customerId = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, Guid tenantId);
    Task<Invoice> AddAsync(Invoice invoice);
    Task<Invoice> UpdateAsync(Invoice invoice);
    Task<bool> DeleteAsync(Guid invoiceId, Guid tenantId);
    Task<bool> IsLockedAsync(Guid invoiceId, Guid tenantId);
    Task<bool> PostInvoiceAsync(Guid invoiceId, Guid tenantId, Guid postedBy);
    Task<bool> ApplyPaymentAsync(Guid invoiceId, decimal amount, Guid tenantId);
    Task<bool> UnapplyPaymentAsync(Guid invoiceId, decimal amount, Guid tenantId);
    /// <summary>Customer email for AR send, or null when missing / wrong tenant.</summary>
    Task<string?> GetCustomerEmailAsync(Guid customerId, Guid tenantId);
    
    // Invoice Lines
    Task<IEnumerable<InvoiceLine>> GetLinesByInvoiceIdAsync(Guid invoiceId, Guid tenantId);
    Task<InvoiceLine?> GetLineByIdAsync(Guid invoiceLineId, Guid tenantId);
    Task<InvoiceLine> AddLineAsync(InvoiceLine invoiceLine);
    Task<InvoiceLine> UpdateLineAsync(InvoiceLine invoiceLine);
    Task<bool> DeleteLineAsync(Guid invoiceLineId, Guid tenantId);
    Task<decimal> GetSubTotalAsync(Guid invoiceId, Guid tenantId);
    Task<decimal> GetTaxTotalAsync(Guid invoiceId, Guid tenantId);
    Task<decimal> GetTotalAmountAsync(Guid invoiceId, Guid tenantId);
    
    // Bills (AP)
    Task<Bill?> GetBillByIdAsync(Guid billId, Guid tenantId);
    Task<IEnumerable<Bill>> GetAllBillsAsync(Guid tenantId, Guid? supplierId = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 50);
    Task<int> GetBillCountAsync(Guid tenantId, Guid? supplierId = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<Bill?> GetBillByBillNumberAsync(string billNumber, Guid tenantId);
    Task<Bill> AddBillAsync(Bill bill);
    Task<Bill> UpdateBillAsync(Bill bill);
    Task<bool> DeleteBillAsync(Guid billId, Guid tenantId);
    Task<bool> IsBillLockedAsync(Guid billId, Guid tenantId);
    Task<bool> PostBillAsync(Guid billId, Guid tenantId, Guid postedBy);
    Task<bool> ApplyPaymentToBillAsync(Guid billId, decimal amount, Guid tenantId);
    Task<bool> UnapplyPaymentFromBillAsync(Guid billId, decimal amount, Guid tenantId);
    
    // Bill Lines
    Task<IEnumerable<BillLine>> GetBillLinesByBillIdAsync(Guid billId, Guid tenantId);
    Task<BillLine?> GetBillLineByIdAsync(Guid billLineId, Guid tenantId);
    Task<BillLine> AddBillLineAsync(BillLine billLine);
    Task<BillLine> UpdateBillLineAsync(BillLine billLine);
    Task<bool> DeleteBillLineAsync(Guid billLineId, Guid tenantId);
    Task<decimal> GetBillSubTotalAsync(Guid billId, Guid tenantId);
    Task<decimal> GetBillTaxTotalAsync(Guid billId, Guid tenantId);
    Task<decimal> GetBillTotalAmountAsync(Guid billId, Guid tenantId);
    
    // Aging Reports
    Task<IEnumerable<Invoice>> GetAgedReceivablesAsync(Guid tenantId, DateTime asOfDate, int[] agingBuckets);
    Task<IEnumerable<Bill>> GetAgedPayablesAsync(Guid tenantId, DateTime asOfDate, int[] agingBuckets);
    Task<IEnumerable<Invoice>> GetCustomerAgingAsync(Guid customerId, Guid tenantId, DateTime asOfDate);
    Task<IEnumerable<Bill>> GetSupplierAgingAsync(Guid supplierId, Guid tenantId, DateTime asOfDate);
}