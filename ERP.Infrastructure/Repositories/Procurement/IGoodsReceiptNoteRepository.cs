using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Repositories.Procurement;

public interface IGoodsReceiptNoteRepository
{
    Task<GoodsReceiptNote?> GetByIdAsync(Guid id);
    Task<IEnumerable<GoodsReceiptNote>> GetAllAsync();
    Task<IEnumerable<GoodsReceiptNote>> GetByVendorAsync(Guid vendorId);
    Task<IEnumerable<GoodsReceiptNote>> GetByStatusAsync(string status);
    Task<GoodsReceiptNote?> GetByNumberAsync(string grnNumber);
    Task<Guid> CreateAsync(GoodsReceiptNote goodsReceiptNote);
    Task<bool> UpdateAsync(GoodsReceiptNote goodsReceiptNote);
    Task<bool> DeleteAsync(Guid id);
}