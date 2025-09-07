using ERP.Domain.Finance;

namespace ERP.Infrastructure.Interfaces.Finance;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id);
    Task<IEnumerable<Invoice>> GetAllAsync();
    Task<Invoice> CreateAsync(Invoice invoice);
    Task<Invoice> UpdateAsync(Invoice invoice);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Invoice?> GetByNumberAsync(string number);
    Task<bool> IsNumberUniqueAsync(string number, Guid? excludeId = null);
}