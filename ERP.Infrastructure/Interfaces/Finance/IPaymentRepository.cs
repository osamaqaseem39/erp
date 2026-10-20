using ERP.Domain.Finance;

namespace ERP.Infrastructure.Interfaces.Finance;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id);
    Task<Payment?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Payment>> GetAllAsync();
    Task<IEnumerable<Payment>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(Payment payment);
    Task<bool> UpdateAsync(Payment payment);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsAsync(Guid id, Guid tenantId);
    Task<Payment?> GetByNumberAsync(string number);
    Task<Payment?> GetByNumberAsync(string number, Guid tenantId);
    Task<bool> IsNumberUniqueAsync(string number, Guid? excludeId = null);
    Task<bool> IsNumberUniqueAsync(string number, Guid tenantId, Guid? excludeId = null);
    Task<IEnumerable<Payment>> GetByInvoiceAsync(Guid invoiceId);
    Task<IEnumerable<Payment>> GetByInvoiceAsync(Guid invoiceId, Guid tenantId);
    Task<IEnumerable<Payment>> GetByBillAsync(Guid billId);
    Task<IEnumerable<Payment>> GetByBillAsync(Guid billId, Guid tenantId);
    Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
}
