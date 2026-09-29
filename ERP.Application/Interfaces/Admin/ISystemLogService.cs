using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Admin;

namespace ERP.Application.Interfaces.Admin;

/// <summary>Read access to the current tenant's system log. Entries are written server-side only.</summary>
public interface ISystemLogService
{
    Task<SystemLogDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<SystemLogDto>> GetAllAsync();
    Task<IEnumerable<SystemLogDto>> GetByLevelAsync(string logLevel);
    Task<IEnumerable<SystemLogDto>> GetByUserAsync(Guid userId);
    Task<IEnumerable<SystemLogDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<PagedResult<SystemLogDto>> GetPagedAsync(SystemLogPagedQuery query, bool allTenants, CancellationToken cancellationToken = default);
}
