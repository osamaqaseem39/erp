using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Interfaces.Procurement;

public interface IGoodsReceiptNoteDetailRepository
{
    Task<IEnumerable<GoodsReceiptNoteDetail>> GetByGrnIdAsync(Guid grnId, Guid tenantId);
    Task<Guid> CreateAsync(GoodsReceiptNoteDetail detail);
    Task<bool> DeleteByGrnIdAsync(Guid grnId, Guid tenantId);
}
