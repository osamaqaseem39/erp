using ERP.Domain.Finance;

namespace ERP.Infrastructure.Interfaces.Finance;

public interface IInvoiceLineRepository
{
    Task<InvoiceLine?> GetByIdAsync(int id);
    Task<IEnumerable<InvoiceLine>> GetAllAsync();
    Task<InvoiceLine> AddAsync(InvoiceLine entity);
    Task UpdateAsync(InvoiceLine entity);
    Task DeleteAsync(int id);
}