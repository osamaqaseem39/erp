using ERP.Domain.Finance;

namespace ERP.Infrastructure.Repositories.Finance;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id);
    Task<IEnumerable<Invoice>> GetAllAsync();
    Task<IEnumerable<Invoice>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<Invoice>> GetByStatusAsync(string status);
    Task<IEnumerable<Invoice>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Invoice?> GetByNumberAsync(string invoiceNumber);
    Task<Guid> CreateAsync(Invoice invoice);
    Task<bool> UpdateAsync(Invoice invoice);
    Task<bool> DeleteAsync(Guid id);
}