using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Interfaces.Procurement;

public interface IPurchaseInvoiceDetailRepository
{
    Task<PurchaseInvoiceDetail?> GetByIdAsync(int id);
    Task<IEnumerable<PurchaseInvoiceDetail>> GetAllAsync();
    Task<PurchaseInvoiceDetail> AddAsync(PurchaseInvoiceDetail entity);
    Task UpdateAsync(PurchaseInvoiceDetail entity);
    Task DeleteAsync(int id);
}