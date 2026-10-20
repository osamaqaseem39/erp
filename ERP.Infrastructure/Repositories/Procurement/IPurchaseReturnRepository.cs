using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Repositories.Procurement;

public interface IPurchaseReturnRepository
{
    Task<PurchaseReturn?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PurchaseReturn>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(PurchaseReturn entity);
    Task<bool> UpdateAsync(PurchaseReturn entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
