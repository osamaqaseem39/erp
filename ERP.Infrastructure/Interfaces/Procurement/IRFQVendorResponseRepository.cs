using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Interfaces.Procurement;

public interface IRFQVendorResponseRepository
{
    Task<RFQVendorResponse?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<RFQVendorResponse>> GetByRfqIdAsync(Guid rfqId, Guid tenantId);
    Task<Guid> CreateAsync(RFQVendorResponse entity);
    Task<bool> UpdateAsync(RFQVendorResponse entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
