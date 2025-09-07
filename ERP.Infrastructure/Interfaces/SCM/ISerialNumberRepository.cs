using ERP.Domain.SCM;

namespace ERP.Infrastructure.Interfaces.SCM;

public interface ISerialNumberRepository
{
    Task<IEnumerable<SerialNumber>> GetByItemAsync(Guid itemId, Guid tenantId, string? status = null);
    Task<SerialNumber?> GetByIdAsync(Guid id, Guid tenantId);
    Task<SerialNumber> CreateAsync(SerialNumber entity);
    Task<SerialNumber> UpdateAsync(SerialNumber entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}