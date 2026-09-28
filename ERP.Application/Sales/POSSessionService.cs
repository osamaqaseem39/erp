using ERP.Application.Interfaces.Sales;
using ERP.Domain.Sales;
using ERP.Infrastructure.Interfaces.Sales;
using ERP.Shared.DTOs.Sales;

namespace ERP.Application.Sales;

public class POSSessionService : IPOSSessionService
{
    private readonly IPOSSessionRepository _repository;

    public POSSessionService(IPOSSessionRepository repository)
    {
        _repository = repository;
    }

    public async Task<POSSessionDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<POSSessionDto>> GetAllAsync(Guid tenantId)
    {
        var entities = await _repository.GetAllAsync(tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreatePOSSessionDto dto, Guid tenantId, Guid userId)
    {
        var entity = MapToEntity(dto, tenantId, userId);
        var created = await _repository.AddAsync(entity);
        return created.Id;
    }

    public async Task<bool> UpdateAsync(UpdatePOSSessionDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;

        MapToEntity(dto, entity, userId);
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<IEnumerable<POSSessionDto>> GetByCashierAsync(Guid cashierId, Guid tenantId)
    {
        var entities = await _repository.GetByCashierAsync(cashierId, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<POSSessionDto>> GetByStatusAsync(string status, Guid tenantId)
    {
        var entities = await _repository.GetByStatusAsync(status, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<bool> CloseSessionAsync(ClosePOSSessionDto dto, Guid tenantId, Guid userId)
    {
        return await _repository.CloseSessionAsync(dto.SessionId, dto.ClosingBalance, dto.Notes, tenantId, userId);
    }

    private POSSessionDto MapToDto(POSSession entity)
    {
        return new POSSessionDto
        {
            Id = entity.Id,
            SessionNumber = entity.SessionNumber,
            CashierId = entity.CashierId,
            TerminalId = entity.TerminalId,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            OpeningBalance = entity.OpeningBalance,
            ClosingBalance = entity.ClosingBalance,
            ExpectedBalance = entity.ExpectedBalance,
            Difference = entity.Difference,
            Status = entity.Status,
            Notes = entity.Notes,
            TenantId = entity.TenantId,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private POSSession MapToEntity(CreatePOSSessionDto dto, Guid tenantId, Guid userId)
    {
        return new POSSession
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SessionNumber = dto.SessionNumber,
            CashierId = dto.CashierId,
            TerminalId = dto.TerminalId,
            StartTime = dto.StartTime,
            OpeningBalance = dto.OpeningBalance,
            Status = dto.Status,
            Notes = dto.Notes,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };
    }

    private void MapToEntity(UpdatePOSSessionDto dto, POSSession entity, Guid userId)
    {
        entity.SessionNumber = dto.SessionNumber;
        entity.CashierId = dto.CashierId;
        entity.TerminalId = dto.TerminalId;
        if (dto.StartTime.HasValue) entity.StartTime = dto.StartTime.Value;
        if (dto.EndTime.HasValue) entity.EndTime = dto.EndTime.Value;
        if (dto.OpeningBalance.HasValue) entity.OpeningBalance = dto.OpeningBalance.Value;
        entity.ClosingBalance = dto.ClosingBalance;
        entity.Status = dto.Status;
        entity.Notes = dto.Notes;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
    }
}

