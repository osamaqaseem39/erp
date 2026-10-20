using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Interfaces.Procurement;

public interface IRFQDetailRepository
{
    Task<RFQDetail?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<RFQDetail>> GetByRfqIdAsync(Guid rfqId, Guid tenantId);
    Task<Guid> CreateAsync(RFQDetail entity);
    Task<bool> UpdateAsync(RFQDetail entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> DeleteByRfqIdAsync(Guid rfqId, Guid tenantId);
}
