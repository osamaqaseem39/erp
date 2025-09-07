using ERP.Shared.DTOs.Admin;

namespace ERP.Application.Interfaces.Admin;

public interface IAuditLogService
{
    Task<AuditLogDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<AuditLogDto>> GetAllAsync();
    Task<IEnumerable<AuditLogDto>> GetByEntityAsync(string entityName, string entityId);
    Task<IEnumerable<AuditLogDto>> GetByUserAsync(Guid userId);
    Task<IEnumerable<AuditLogDto>> GetByActionAsync(string action);
    Task<IEnumerable<AuditLogDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Guid> CreateAsync(CreateAuditLogDto dto);
    Task<bool> LogActionAsync(string entityName, Guid entityId, string action, string? oldValues, string? newValues, Guid? userId);
}