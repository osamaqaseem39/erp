using ERP.Domain.Finance;

namespace ERP.Infrastructure.Interfaces.Finance;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Payment>> GetAllAsync();
    Task<Guid> CreateAsync(Payment payment);
    Task<bool> UpdateAsync(Payment payment);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Payment?> GetByNumberAsync(string number);
    Task<bool> IsNumberUniqueAsync(string number, Guid? excludeId = null);
    Task<IEnumerable<Payment>> GetByInvoiceAsync(Guid invoiceId);
    Task<IEnumerable<Payment>> GetByBillAsync(Guid billId);
    Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}