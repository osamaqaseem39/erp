using ERP.Shared.DTOs.Procurement;

namespace ERP.Application.Interfaces.Procurement;

public interface IPurchaseOrderService
{
    Task<PurchaseOrderDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PurchaseOrderDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreatePurchaseOrderDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdatePurchaseOrderDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PurchaseOrderDto>> GetByVendorAsync(Guid vendorId, Guid tenantId);
    Task<IEnumerable<PurchaseOrderDto>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<PurchaseOrderDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
    Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> RejectAsync(Guid id, Guid tenantId, Guid userId);
}
