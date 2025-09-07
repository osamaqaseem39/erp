using ERP.Domain.Admin;

namespace ERP.Infrastructure.Repositories.Admin;

public interface ISystemLogRepository
{
    Task<SystemLog?> GetByIdAsync(Guid id);
    Task<IEnumerable<SystemLog>> GetAllAsync();
    Task<IEnumerable<SystemLog>> GetByLevelAsync(string logLevel);
    Task<IEnumerable<SystemLog>> GetByUserAsync(Guid userId);
    Task<IEnumerable<SystemLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Guid> CreateAsync(SystemLog systemLog);
    Task<bool> UpdateAsync(SystemLog systemLog);
    Task<bool> DeleteAsync(Guid id);
}