using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Admin;

namespace ERP.Application.Interfaces.Admin;

/// <summary>Read access to the current tenant's audit trail. Entries are written server-side only.</summary>
public interface IAuditLogService
{
    Task<AuditLogDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<AuditLogDto>> GetAllAsync();
    Task<IEnumerable<AuditLogDto>> GetByEntityAsync(string entityName, string entityId);
    Task<IEnumerable<AuditLogDto>> GetByUserAsync(Guid userId);
    Task<IEnumerable<AuditLogDto>> GetByActionAsync(string action);
    Task<IEnumerable<AuditLogDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<PagedResult<AuditLogDto>> GetPagedAsync(AuditLogPagedQuery query, bool allTenants, CancellationToken cancellationToken = default);
}
