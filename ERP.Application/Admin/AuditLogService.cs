using ERP.Application.Interfaces.Admin;
using ERP.Domain.Core;
using ERP.Infrastructure.Repositories.Admin;
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

    public async Task<IEnumerable<AuditLogDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<AuditLogDto>> GetByEntityAsync(string entityName, string entityId)
    {
        var entities = await _repository.GetByEntityAsync(entityName, entityId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<AuditLogDto>> GetByUserAsync(Guid userId)
    {
        var entities = await _repository.GetByUserAsync(userId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<AuditLogDto>> GetByActionAsync(string action)
    {
        var entities = await _repository.GetByActionAsync(action);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<AuditLogDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var entities = await _repository.GetByDateRangeAsync(startDate, endDate);
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateAuditLogDto dto)
    {
        var entity = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = dto.TenantId ?? Guid.Empty,
            UserId = dto.UserId ?? Guid.Empty,
            Action = dto.Action,
            EntityName = dto.EntityName,
            EntityId = dto.EntityId.ToString(),
            OldValues = dto.OldValues ?? string.Empty,
            NewValues = dto.NewValues ?? string.Empty,
            IpAddress = dto.IPAddress ?? string.Empty,
            UserAgent = dto.UserAgent ?? string.Empty,
            Timestamp = DateTime.UtcNow,
        };

        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> LogActionAsync(
        string entityName,
        Guid entityId,
        string action,
        string? oldValues,
        string? newValues,
        Guid? userId)
    {
        var dto = new CreateAuditLogDto
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            OldValues = oldValues,
            NewValues = newValues,
            UserId = userId,
        };

        await CreateAsync(dto);
        return true;
    }

    private static AuditLogDto MapToDto(AuditLog entity)
    {
        _ = Guid.TryParse(entity.EntityId, out var entityId);

        return new AuditLogDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            EntityName = entity.EntityName,
            EntityId = entityId,
            Action = entity.Action,
            OldValues = string.IsNullOrWhiteSpace(entity.OldValues) ? null : entity.OldValues,
            NewValues = string.IsNullOrWhiteSpace(entity.NewValues) ? null : entity.NewValues,
            UserId = entity.UserId,
            IPAddress = entity.IpAddress,
            UserAgent = entity.UserAgent,
            Timestamp = entity.Timestamp,
        };
    }
}
