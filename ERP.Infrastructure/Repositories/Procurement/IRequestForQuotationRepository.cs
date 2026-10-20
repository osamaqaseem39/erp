using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Repositories.Procurement;

public interface IRequestForQuotationRepository
{
    Task<RequestForQuotation?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<RequestForQuotation>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<RequestForQuotation>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<RequestForQuotation>> GetByRequestorAsync(Guid requestorId, Guid tenantId);
    Task<Guid> CreateAsync(RequestForQuotation requestForQuotation);
    Task<bool> UpdateAsync(RequestForQuotation requestForQuotation);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
