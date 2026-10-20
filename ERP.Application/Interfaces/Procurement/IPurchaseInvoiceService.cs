using ERP.Shared.DTOs.Procurement;

namespace ERP.Application.Interfaces.Procurement;

public interface IPurchaseInvoiceService
{
    Task<PurchaseInvoiceDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PurchaseInvoiceDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreatePurchaseInvoiceDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdatePurchaseInvoiceDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PurchaseInvoiceDto>> GetByVendorAsync(Guid vendorId, Guid tenantId);
    Task<IEnumerable<PurchaseInvoiceDto>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<PurchaseInvoiceDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
    Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> RejectAsync(Guid id, Guid tenantId, Guid userId);
}
