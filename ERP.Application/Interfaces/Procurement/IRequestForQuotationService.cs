using ERP.Shared.DTOs.Procurement;

namespace ERP.Application.Interfaces.Procurement;

public interface IRequestForQuotationService
{
    Task<RequestForQuotationDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<RequestForQuotationDto>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<RequestForQuotationDto>> GetByStatusAsync(string status, Guid tenantId);
    Task<Guid> CreateAsync(CreateRequestForQuotationDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateRequestForQuotationDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    /// <summary>Draft → Open (issued to vendors).</summary>
    Task<bool> SendAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> CloseAsync(Guid id, Guid tenantId, Guid userId);
    Task<Guid> SubmitVendorResponseAsync(CreateRFQVendorResponseDto dto, Guid tenantId, Guid userId);
    /// <summary>Open → Awarded; marks chosen response Accepted, others Rejected.</summary>
    Task<bool> AwardAsync(Guid rfqId, Guid vendorResponseId, Guid tenantId, Guid userId);
}
