using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Interfaces;

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
}