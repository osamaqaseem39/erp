using ERP.Application.Interfaces.Admin;
using ERP.Domain.Admin;
using ERP.Infrastructure.Repositories.Admin;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Admin;

namespace ERP.Application.Admin;

public class SystemLogService : ISystemLogService
{
    private readonly ISystemLogRepository _repository;

    public SystemLogService(ISystemLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<SystemLogDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<SystemLogDto>> GetAllAsync() => (await _repository.GetAllAsync()).Select(MapToDto);

    public async Task<IEnumerable<SystemLogDto>> GetByLevelAsync(string logLevel) => (await _repository.GetByLevelAsync(logLevel)).Select(MapToDto);

    public async Task<IEnumerable<SystemLogDto>> GetByUserAsync(Guid userId) => (await _repository.GetByUserAsync(userId)).Select(MapToDto);

    public async Task<IEnumerable<SystemLogDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate) =>
        (await _repository.GetByDateRangeAsync(startDate, endDate)).Select(MapToDto);

    public async Task<PagedResult<SystemLogDto>> GetPagedAsync(SystemLogPagedQuery query, bool allTenants, CancellationToken cancellationToken = default) =>
        (await _repository.GetPagedAsync(query, allTenants, cancellationToken)).Map(MapToDto);

    private static SystemLogDto MapToDto(SystemLog entity) => new()
    {
        Id = entity.Id,
        TenantId = entity.TenantId,
        LogLevel = entity.LogLevel,
        Message = entity.Message,
        Exception = entity.Exception,
        Source = entity.Source,
        UserId = entity.UserId?.ToString(),
        IPAddress = entity.IPAddress,
        AdditionalData = entity.Properties,
        CreatedAt = entity.Timestamp,
    };
}
