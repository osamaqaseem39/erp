using ERP.Application.Interfaces.Admin;
using ERP.Domain.Admin;
using ERP.Infrastructure.Repositories.Admin;
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

    public async Task<IEnumerable<SystemLogDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<SystemLogDto>> GetByLevelAsync(string logLevel)
    {
        var entities = await _repository.GetByLevelAsync(logLevel);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<SystemLogDto>> GetByUserAsync(Guid userId)
    {
        var entities = await _repository.GetByUserAsync(userId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<SystemLogDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var entities = await _repository.GetByDateRangeAsync(startDate, endDate);
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateSystemLogDto dto)
    {
        var entity = new SystemLog
        {
            Id = Guid.NewGuid(),
            TenantId = dto.TenantId,
            LogLevel = string.IsNullOrWhiteSpace(dto.LogLevel) ? dto.Level : dto.LogLevel,
            Message = dto.Message,
            Exception = dto.Exception,
            Source = dto.Source,
            UserId = Guid.TryParse(dto.UserId, out var userId) ? userId : null,
            IPAddress = dto.IPAddress,
            Timestamp = DateTime.UtcNow,
            Properties = dto.AdditionalData,
        };

        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateSystemLogDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id);
        if (entity == null)
            return false;

        if (!string.IsNullOrWhiteSpace(dto.LogLevel))
            entity.LogLevel = dto.LogLevel;
        if (!string.IsNullOrWhiteSpace(dto.Message))
            entity.Message = dto.Message;
        entity.Exception = dto.Exception ?? entity.Exception;
        entity.Source = dto.Source ?? entity.Source;
        entity.UpdatedAt = DateTime.UtcNow;

        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }

    private static SystemLogDto MapToDto(SystemLog entity)
    {
        return new SystemLogDto
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
}
