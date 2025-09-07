using ERP.Domain.Core;

namespace ERP.Infrastructure.Repositories.Admin;

public interface IAuditLogRepository
{
    Task<AuditLog?> GetByIdAsync(Guid id);
    Task<IEnumerable<AuditLog>> GetAllAsync();
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityName, string entityId);
    Task<IEnumerable<AuditLog>> GetByUserAsync(Guid userId);
    Task<IEnumerable<AuditLog>> GetByActionAsync(string action);
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Guid> CreateAsync(AuditLog auditLog);
    Task<bool> UpdateAsync(AuditLog auditLog);
    Task<bool> DeleteAsync(Guid id);
}