using ERP.Shared.DTOs.Sales;

namespace ERP.Application.Interfaces.Sales;

public interface ISalesCreditNoteService
{
    Task<SalesCreditNoteDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<SalesCreditNoteDto>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<SalesCreditNoteDto>> GetByCustomerAsync(Guid customerId, Guid tenantId);
    Task<IEnumerable<SalesCreditNoteDto>> GetByStatusAsync(string status, Guid tenantId);
    Task<Guid> CreateAsync(CreateSalesCreditNoteDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateSalesCreditNoteDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> PostAsync(Guid creditNoteId, Guid tenantId, Guid postedBy);
    Task<bool> VoidAsync(Guid creditNoteId, VoidSalesCreditNoteDto dto, Guid tenantId, Guid voidedBy);
}
