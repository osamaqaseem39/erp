using ERP.Domain.Admin;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Admin;

namespace ERP.Infrastructure.Repositories.Admin;

/// <summary>Tenant-scoped. API callers can only read; entries come from the server (errors, security events).</summary>
public interface ISystemLogRepository
{
    Task<SystemLog?> GetByIdAsync(Guid id);
    Task<IEnumerable<SystemLog>> GetAllAsync();
    Task<IEnumerable<SystemLog>> GetByLevelAsync(string logLevel);
    Task<IEnumerable<SystemLog>> GetByUserAsync(Guid userId);
    Task<IEnumerable<SystemLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<PagedResult<SystemLog>> GetPagedAsync(SystemLogPagedQuery query, bool allTenants = false, CancellationToken cancellationToken = default);
}
