using ERP.Domain.Finance;

namespace ERP.Infrastructure.Interfaces.Finance;

public interface IPaymentApplicationRepository
{
    Task<PaymentApplication?> GetByIdAsync(int id);
    Task<IEnumerable<PaymentApplication>> GetAllAsync();
    Task<PaymentApplication> AddAsync(PaymentApplication entity);
    Task UpdateAsync(PaymentApplication entity);
    Task DeleteAsync(int id);
}