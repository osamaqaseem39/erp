using ERP.Domain.Finance;

namespace ERP.Application.Interfaces.Finance;

public interface IPaymentRepository
{
    // Payments
    Task<Payment?> GetByIdAsync(Guid paymentId, Guid tenantId);
    Task<IEnumerable<Payment>> GetAllAsync(Guid tenantId, string? status = null, string? paymentMethod = null, DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 50);
    Task<int> GetCountAsync(Guid tenantId, string? status = null, string? paymentMethod = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<Payment?> GetByPaymentNumberAsync(string paymentNumber, Guid tenantId);
    Task<Payment> AddAsync(Payment payment);
    Task<Payment> UpdateAsync(Payment payment);
    Task<bool> DeleteAsync(Guid paymentId, Guid tenantId);
    Task<bool> UpdateStatusAsync(Guid paymentId, string status, Guid tenantId, Guid userId);
    Task<bool> ProcessPaymentAsync(Guid paymentId, Guid tenantId, Guid userId);
    Task<bool> ReconcilePaymentAsync(Guid paymentId, Guid tenantId, Guid userId);
    
    // Payment Application
    Task<IEnumerable<Payment>> GetPaymentsForInvoiceAsync(Guid invoiceId, Guid tenantId);
    Task<IEnumerable<Payment>> GetPaymentsForBillAsync(Guid billId, Guid tenantId);
    Task<bool> ApplyPaymentToInvoiceAsync(Guid paymentId, Guid invoiceId, decimal amount, Guid tenantId, Guid userId);
    Task<bool> ApplyPaymentToBillAsync(Guid paymentId, Guid billId, decimal amount, Guid tenantId, Guid userId);
    Task<decimal> GetAppliedAmountAsync(Guid paymentId, Guid tenantId);
    Task<decimal> GetAvailableAmountAsync(Guid paymentId, Guid tenantId);
    
    // Payment Methods
    Task<IEnumerable<string>> GetAvailablePaymentMethodsAsync();
    Task<bool> ValidatePaymentMethodAsync(string paymentMethod, decimal amount, string currencyCode);
    
    // Payment Processing
    Task<IEnumerable<Payment>> GetPendingPaymentsAsync(Guid tenantId);
    Task<IEnumerable<Payment>> GetFailedPaymentsAsync(Guid tenantId);
    Task<bool> RetryPaymentAsync(Guid paymentId, Guid tenantId, Guid userId);
}