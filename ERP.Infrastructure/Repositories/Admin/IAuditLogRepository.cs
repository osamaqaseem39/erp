using ERP.Domain.Core;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Admin;

namespace ERP.Infrastructure.Repositories.Admin;

/// <summary>Tenant-scoped and read-only: entries are written by the server's audit interceptor, never by API callers.</summary>
public interface IAuditLogRepository
{
    Task<AuditLog?> GetByIdAsync(Guid id);
    Task<IEnumerable<AuditLog>> GetAllAsync();
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityName, string entityId);
    Task<IEnumerable<AuditLog>> GetByUserAsync(Guid userId);
    Task<IEnumerable<AuditLog>> GetByActionAsync(string action);
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<PagedResult<AuditLog>> GetPagedAsync(AuditLogPagedQuery query, bool allTenants = false, CancellationToken cancellationToken = default);
}
