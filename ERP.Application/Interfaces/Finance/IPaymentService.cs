using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Interfaces.Finance;

public interface IPaymentService
{
    Task<PaymentDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<PaymentDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreatePaymentDto dto);
    Task<bool> UpdateAsync(UpdatePaymentDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<PaymentDto>> GetByInvoiceAsync(Guid invoiceId);
    Task<IEnumerable<PaymentDto>> GetByBillAsync(Guid billId);
    Task<IEnumerable<PaymentDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> ProcessPaymentAsync(Guid id);
    
    // Additional methods for GraphQL mutations
    Task<Guid> AddPaymentAsync(CreatePaymentDto dto, Guid tenantId, Guid userId);
    Task<Guid> UpdatePaymentAsync(UpdatePaymentDto dto, Guid tenantId, Guid userId);
    Task<bool> ProcessPaymentAsync(Guid paymentId, Guid tenantId, Guid userId);
    Task<bool> ReconcilePaymentAsync(Guid paymentId, Guid tenantId, Guid userId);
    Task<bool> ApplyPaymentToInvoiceAsync(PaymentApplicationDto dto, Guid tenantId, Guid userId);
    Task<bool> ApplyPaymentToBillAsync(PaymentApplicationDto dto, Guid tenantId, Guid userId);
}