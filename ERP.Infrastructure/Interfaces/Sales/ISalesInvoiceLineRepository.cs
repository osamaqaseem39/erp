using ERP.Domain.Sales;

namespace ERP.Infrastructure.Interfaces.Sales;

public interface ISalesInvoiceLineRepository
{
    Task<SalesInvoiceLine?> GetByIdAsync(int id);
    Task<IEnumerable<SalesInvoiceLine>> GetAllAsync();
    Task<SalesInvoiceLine> AddAsync(SalesInvoiceLine entity);
    Task UpdateAsync(SalesInvoiceLine entity);
    Task DeleteAsync(int id);
}