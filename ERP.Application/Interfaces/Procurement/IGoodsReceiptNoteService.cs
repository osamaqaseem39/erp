using ERP.Shared.DTOs.Procurement;

namespace ERP.Application.Interfaces.Procurement;

public interface IGoodsReceiptNoteService
{
    Task<GoodsReceiptNoteDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<GoodsReceiptNoteDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateGoodsReceiptNoteDto dto);
    Task<bool> UpdateAsync(UpdateGoodsReceiptNoteDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<GoodsReceiptNoteDto>> GetByPurchaseOrderAsync(Guid purchaseOrderId);
    Task<IEnumerable<GoodsReceiptNoteDto>> GetByStatusAsync(string status);
    Task<IEnumerable<GoodsReceiptNoteDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> ApproveAsync(Guid id);
    Task<bool> RejectAsync(Guid id);
}