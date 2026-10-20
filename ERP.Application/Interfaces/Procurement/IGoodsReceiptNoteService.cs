using ERP.Shared.DTOs.Procurement;

namespace ERP.Application.Interfaces.Procurement;

public interface IGoodsReceiptNoteService
{
    Task<GoodsReceiptNoteDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<GoodsReceiptNoteDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateGoodsReceiptNoteDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateGoodsReceiptNoteDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<GoodsReceiptNoteDto>> GetByPurchaseOrderAsync(Guid purchaseOrderId, Guid tenantId);
    Task<IEnumerable<GoodsReceiptNoteDto>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<GoodsReceiptNoteDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
    Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> RejectAsync(Guid id, Guid tenantId, Guid userId);
}
