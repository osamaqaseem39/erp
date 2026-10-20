using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Interfaces.Finance;

public interface IPaymentService
{
    Task<PaymentDto?> GetByIdAsync(Guid id);
    Task<PaymentDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PaymentDto>> GetAllAsync();
    Task<IEnumerable<PaymentDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreatePaymentDto dto);
    Task<bool> UpdateAsync(UpdatePaymentDto dto);
    Task<bool> UpdateAsync(UpdatePaymentDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PaymentDto>> GetByInvoiceAsync(Guid invoiceId);
    Task<IEnumerable<PaymentDto>> GetByInvoiceAsync(Guid invoiceId, Guid tenantId);
    Task<IEnumerable<PaymentDto>> GetByBillAsync(Guid billId);
    Task<IEnumerable<PaymentDto>> GetByBillAsync(Guid billId, Guid tenantId);
    Task<IEnumerable<PaymentDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<PaymentDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
    Task<bool> ProcessPaymentAsync(Guid id);

    Task<Guid> AddPaymentAsync(CreatePaymentDto dto, Guid tenantId, Guid userId);
    Task<Guid> UpdatePaymentAsync(UpdatePaymentDto dto, Guid tenantId, Guid userId);
    Task<bool> ProcessPaymentAsync(Guid paymentId, Guid tenantId, Guid userId);
    Task<bool> ReconcilePaymentAsync(Guid paymentId, Guid tenantId, Guid userId);
    Task<bool> ApplyPaymentToInvoiceAsync(PaymentApplicationDto dto, Guid tenantId, Guid userId);
    Task<bool> ApplyPaymentToBillAsync(PaymentApplicationDto dto, Guid tenantId, Guid userId);
    Task<bool> VoidAsync(Guid paymentId, VoidPaymentDto dto, Guid tenantId, Guid userId);
    Task<bool> RefundAsync(Guid paymentId, RefundDto dto, Guid tenantId, Guid userId);
}