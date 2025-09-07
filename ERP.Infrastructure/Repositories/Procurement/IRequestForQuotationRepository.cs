using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Repositories.Procurement;

public interface IRequestForQuotationRepository
{
    Task<RequestForQuotation?> GetByIdAsync(Guid id);
    Task<IEnumerable<RequestForQuotation>> GetAllAsync();
    Task<IEnumerable<RequestForQuotation>> GetByStatusAsync(string status);
    Task<IEnumerable<RequestForQuotation>> GetByRequestorAsync(Guid requestorId);
    Task<Guid> CreateAsync(RequestForQuotation requestForQuotation);
    Task<bool> UpdateAsync(RequestForQuotation requestForQuotation);
    Task<bool> DeleteAsync(Guid id);
}