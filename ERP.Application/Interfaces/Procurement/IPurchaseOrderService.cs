using ERP.Shared.DTOs.Procurement;

namespace ERP.Application.Interfaces.Procurement;

public interface IPurchaseOrderService
{
    Task<PurchaseOrderDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<PurchaseOrderDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreatePurchaseOrderDto dto);
    Task<bool> UpdateAsync(UpdatePurchaseOrderDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<PurchaseOrderDto>> GetByVendorAsync(Guid vendorId);
    Task<IEnumerable<PurchaseOrderDto>> GetByStatusAsync(string status);
    Task<IEnumerable<PurchaseOrderDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> ApproveAsync(Guid id);
    Task<bool> RejectAsync(Guid id);
}