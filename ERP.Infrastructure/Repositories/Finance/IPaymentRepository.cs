using ERP.Domain.Finance;

namespace ERP.Infrastructure.Repositories.Finance;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Payment>> GetAllAsync();
    Task<IEnumerable<Payment>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<Payment>> GetBySupplierAsync(Guid supplierId);
    Task<IEnumerable<Payment>> GetByStatusAsync(string status);
    Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Payment?> GetByNumberAsync(string paymentNumber);
    Task<Guid> CreateAsync(Payment payment);
    Task<bool> UpdateAsync(Payment payment);
    Task<bool> DeleteAsync(Guid id);
}