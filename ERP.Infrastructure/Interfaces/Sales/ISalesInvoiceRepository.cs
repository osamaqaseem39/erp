using ERP.Domain.Sales;

namespace ERP.Infrastructure.Interfaces.Sales;

public interface ISalesInvoiceRepository
{
    Task<SalesInvoice?> GetByIdAsync(int id);
    Task<IEnumerable<SalesInvoice>> GetAllAsync();
    Task<SalesInvoice> AddAsync(SalesInvoice entity);
    Task UpdateAsync(SalesInvoice entity);
    Task DeleteAsync(int id);
}