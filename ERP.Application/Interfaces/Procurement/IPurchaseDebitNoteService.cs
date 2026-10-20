using ERP.Shared.DTOs.Procurement;

namespace ERP.Application.Interfaces.Procurement;

public interface IPurchaseDebitNoteService
{
    Task<PurchaseDebitNoteDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PurchaseDebitNoteDto>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<PurchaseDebitNoteDto>> GetByVendorAsync(Guid vendorId, Guid tenantId);
    Task<IEnumerable<PurchaseDebitNoteDto>> GetByStatusAsync(string status, Guid tenantId);
    Task<Guid> CreateAsync(CreatePurchaseDebitNoteDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdatePurchaseDebitNoteDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> PostAsync(Guid debitNoteId, Guid tenantId, Guid postedBy);
    Task<bool> VoidAsync(Guid debitNoteId, VoidPurchaseDebitNoteDto dto, Guid tenantId, Guid voidedBy);
}
