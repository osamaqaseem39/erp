using ERP.Domain.Sales;

namespace ERP.Infrastructure.Interfaces.Sales;

public interface ISalesCreditNoteLineRepository
{
    Task<IEnumerable<SalesCreditNoteLine>> GetByCreditNoteIdAsync(Guid creditNoteId, Guid tenantId);
    Task<Guid> CreateAsync(SalesCreditNoteLine line);
    Task<bool> DeleteByCreditNoteIdAsync(Guid creditNoteId, Guid tenantId);
}
