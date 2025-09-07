using ERP.Shared.DTOs.Core;

namespace ERP.Infrastructure.Services;

public interface IAuditService
{
    Task LogAsync(string action, string entityName, string entityId, string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null);
    Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(Guid tenantId, int pageNumber = 1, int pageSize = 50);
    Task<AuditLogDto?> GetAuditLogByIdAsync(Guid id);
}