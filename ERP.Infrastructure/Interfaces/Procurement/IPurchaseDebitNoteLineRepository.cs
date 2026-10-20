using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Interfaces.Procurement;

public interface IPurchaseDebitNoteLineRepository
{
    Task<IEnumerable<PurchaseDebitNoteLine>> GetByDebitNoteIdAsync(Guid debitNoteId, Guid tenantId);
    Task<Guid> CreateAsync(PurchaseDebitNoteLine line);
    Task<bool> DeleteByDebitNoteIdAsync(Guid debitNoteId, Guid tenantId);
}
