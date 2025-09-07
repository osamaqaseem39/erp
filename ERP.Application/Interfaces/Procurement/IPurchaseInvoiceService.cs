using ERP.Shared.DTOs.Procurement;

namespace ERP.Application.Interfaces.Procurement;

public interface IPurchaseInvoiceService
{
    Task<PurchaseInvoiceDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<PurchaseInvoiceDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreatePurchaseInvoiceDto dto);
    Task<bool> UpdateAsync(UpdatePurchaseInvoiceDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<PurchaseInvoiceDto>> GetByVendorAsync(Guid vendorId);
    Task<IEnumerable<PurchaseInvoiceDto>> GetByStatusAsync(string status);
    Task<IEnumerable<PurchaseInvoiceDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> ApproveAsync(Guid id);
    Task<bool> RejectAsync(Guid id);
}