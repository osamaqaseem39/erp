using ERP.Application.Interfaces.Admin;
using ERP.Domain.Core;
using ERP.Infrastructure.Repositories.Admin;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Admin;

namespace ERP.Application.Admin;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;

    public AuditLogService(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<AuditLogDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<AuditLogDto>> GetAllAsync() => (await _repository.GetAllAsync()).Select(MapToDto);

    public async Task<IEnumerable<AuditLogDto>> GetByEntityAsync(string entityName, string entityId) =>
        (await _repository.GetByEntityAsync(entityName, entityId)).Select(MapToDto);

    public async Task<IEnumerable<AuditLogDto>> GetByUserAsync(Guid userId) => (await _repository.GetByUserAsync(userId)).Select(MapToDto);

    public async Task<IEnumerable<AuditLogDto>> GetByActionAsync(string action) => (await _repository.GetByActionAsync(action)).Select(MapToDto);

    public async Task<IEnumerable<AuditLogDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate) =>
        (await _repository.GetByDateRangeAsync(startDate, endDate)).Select(MapToDto);

    public async Task<PagedResult<AuditLogDto>> GetPagedAsync(AuditLogPagedQuery query, bool allTenants, CancellationToken cancellationToken = default) =>
        (await _repository.GetPagedAsync(query, allTenants, cancellationToken)).Map(MapToDto);

    private static AuditLogDto MapToDto(AuditLog entity) => new()
    {
        Id = entity.Id,
        TenantId = entity.TenantId,
        EntityName = entity.EntityName,
        EntityId = entity.EntityId,
        Action = entity.Action,
        OldValues = string.IsNullOrWhiteSpace(entity.OldValues) ? null : entity.OldValues,
        NewValues = string.IsNullOrWhiteSpace(entity.NewValues) ? null : entity.NewValues,
        UserId = entity.UserId == Guid.Empty ? null : entity.UserId,
        IPAddress = entity.IpAddress,
        UserAgent = entity.UserAgent,
        Timestamp = entity.Timestamp,
    };
}
