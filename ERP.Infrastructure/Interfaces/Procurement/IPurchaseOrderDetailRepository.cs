using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Interfaces.Procurement;

public interface IPurchaseOrderDetailRepository
{
    Task<PurchaseOrderDetail?> GetByIdAsync(int id);
    Task<IEnumerable<PurchaseOrderDetail>> GetAllAsync();
    Task<PurchaseOrderDetail> AddAsync(PurchaseOrderDetail entity);
    Task UpdateAsync(PurchaseOrderDetail entity);
    Task DeleteAsync(int id);
}