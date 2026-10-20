using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Repositories.Procurement;

public interface IGoodsReceiptNoteRepository
{
    Task<GoodsReceiptNote?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<GoodsReceiptNote>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<GoodsReceiptNote>> GetByPurchaseOrderAsync(Guid poId, Guid tenantId);
    Task<IEnumerable<GoodsReceiptNote>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<GoodsReceiptNote>> GetByDateRangeAsync(DateTime start, DateTime end, Guid tenantId);
    Task<Guid> CreateAsync(GoodsReceiptNote goodsReceiptNote);
    Task<bool> UpdateAsync(GoodsReceiptNote goodsReceiptNote);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
