using ERP.Domain.Finance;

namespace ERP.Infrastructure.Repositories.Finance;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id);
    Task<Payment?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Payment>> GetAllAsync();
    Task<IEnumerable<Payment>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<Payment>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<Payment>> GetByCustomerAsync(Guid customerId, Guid tenantId);
    Task<IEnumerable<Payment>> GetBySupplierAsync(Guid supplierId);
    Task<IEnumerable<Payment>> GetBySupplierAsync(Guid supplierId, Guid tenantId);
    Task<IEnumerable<Payment>> GetByStatusAsync(string status);
    Task<IEnumerable<Payment>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
    Task<IEnumerable<Payment>> GetByInvoiceAsync(Guid invoiceId, Guid tenantId);
    Task<IEnumerable<Payment>> GetByBillAsync(Guid billId, Guid tenantId);
    Task<Payment?> GetByNumberAsync(string paymentNumber);
    Task<Payment?> GetByNumberAsync(string paymentNumber, Guid tenantId);
    Task<Guid> CreateAsync(Payment payment);
    Task<bool> UpdateAsync(Payment payment);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
