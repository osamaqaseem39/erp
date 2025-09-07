using ERP.Domain.SCM;

namespace ERP.Infrastructure.Interfaces.SCM;

public interface IBatchSerialTrackingRepository
{
    Task<BatchSerialTracking?> GetByIdAsync(int id);
    Task<IEnumerable<BatchSerialTracking>> GetAllAsync();
    Task<BatchSerialTracking> AddAsync(BatchSerialTracking entity);
    Task UpdateAsync(BatchSerialTracking entity);
    Task DeleteAsync(int id);
}