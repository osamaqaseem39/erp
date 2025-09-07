using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Interfaces.Procurement;

public interface IPurchaseRequisitionDetailRepository
{
    Task<PurchaseRequisitionDetail?> GetByIdAsync(int id);
    Task<IEnumerable<PurchaseRequisitionDetail>> GetAllAsync();
    Task<PurchaseRequisitionDetail> AddAsync(PurchaseRequisitionDetail entity);
    Task UpdateAsync(PurchaseRequisitionDetail entity);
    Task DeleteAsync(int id);
}