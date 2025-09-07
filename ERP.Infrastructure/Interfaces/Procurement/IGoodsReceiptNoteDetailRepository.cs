using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Interfaces.Procurement;

public interface IGoodsReceiptNoteDetailRepository
{
    Task<GoodsReceiptNoteDetail?> GetByIdAsync(int id);
    Task<IEnumerable<GoodsReceiptNoteDetail>> GetAllAsync();
    Task<GoodsReceiptNoteDetail> AddAsync(GoodsReceiptNoteDetail entity);
    Task UpdateAsync(GoodsReceiptNoteDetail entity);
    Task DeleteAsync(int id);
}