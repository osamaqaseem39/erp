using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Interfaces.Procurement;

public interface IPurchaseDebitNoteRepository
{
    Task<PurchaseDebitNote?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PurchaseDebitNote>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<PurchaseDebitNote>> GetByVendorAsync(Guid vendorId, Guid tenantId);
    Task<IEnumerable<PurchaseDebitNote>> GetByStatusAsync(string status, Guid tenantId);
    Task<PurchaseDebitNote> AddAsync(PurchaseDebitNote entity);
    Task UpdateAsync(PurchaseDebitNote entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
