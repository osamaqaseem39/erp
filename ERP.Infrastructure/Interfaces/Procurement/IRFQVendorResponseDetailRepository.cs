using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Interfaces.Procurement;

public interface IRFQVendorResponseDetailRepository
{
    Task<RFQVendorResponseDetail?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<RFQVendorResponseDetail>> GetByResponseAsync(Guid responseId, Guid tenantId);
    Task<Guid> CreateAsync(RFQVendorResponseDetail entity);
    Task<bool> UpdateAsync(RFQVendorResponseDetail entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
