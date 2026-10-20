using ERP.Application.Common;
using ERP.Application.Interfaces.Sales;
using ERP.Domain.Sales;
using ERP.Infrastructure.Interfaces.Sales;
using ERP.Shared.DTOs.Sales;

namespace ERP.Application.Sales;

public class POSSessionService : IPOSSessionService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Open"] = new[] { "Closed" },
            ["Closed"] = Array.Empty<string>()
        };

    private readonly IPOSSessionRepository _repository;

    public POSSessionService(IPOSSessionRepository repository)
    {
        _repository = repository;
    }

    public async Task<POSSessionDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<POSSessionDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return (await _repository.GetAllAsync(tenantId)).Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreatePOSSessionDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        var entity = MapToEntity(dto, tenantId, userId);
        // ERP-FIX-5: always start Open
        entity.Status = "Open";
        var created = await _repository.AddAsync(entity);
        return created.Id;
    }

    public async Task<bool> UpdateAsync(UpdatePOSSessionDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        if (string.Equals(entity.Status, "Closed", StringComparison.OrdinalIgnoreCase))
            return false;

        MapToEntity(dto, entity, userId);
        // ERP-FIX-5: status only via CloseSessionAsync
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (string.Equals(entity.Status, "Closed", StringComparison.OrdinalIgnoreCase))
            return false;
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<IEnumerable<POSSessionDto>> GetByCashierAsync(Guid cashierId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return (await _repository.GetByCashierAsync(cashierId, tenantId)).Select(MapToDto);
    }

    public async Task<IEnumerable<POSSessionDto>> GetByStatusAsync(string status, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return (await _repository.GetByStatusAsync(status, tenantId)).Select(MapToDto);
    }

    public async Task<bool> CloseSessionAsync(ClosePOSSessionDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(dto.SessionId, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Closed", Transitions))
            return false;

        return await _repository.CloseSessionAsync(dto.SessionId, dto.ClosingBalance, dto.Notes, tenantId, userId);
    }

    private static POSSessionDto MapToDto(POSSession entity)
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

    private static POSSession MapToEntity(CreatePOSSessionDto dto, Guid tenantId, Guid userId)
    {
        return new POSSession
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SessionNumber = dto.SessionNumber,
            CashierId = dto.CashierId,
            TerminalId = dto.TerminalId,
            StartTime = dto.StartTime == default ? DateTime.UtcNow : dto.StartTime,
            OpeningBalance = dto.OpeningBalance,
            Status = "Open",
            Notes = dto.Notes,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };
    }

    private static void MapToEntity(UpdatePOSSessionDto dto, POSSession entity, Guid userId)
    {
        entity.SessionNumber = dto.SessionNumber;
        entity.CashierId = dto.CashierId;
        entity.TerminalId = dto.TerminalId;
        if (dto.StartTime.HasValue) entity.StartTime = dto.StartTime.Value;
        if (dto.EndTime.HasValue) entity.EndTime = dto.EndTime.Value;
        if (dto.OpeningBalance.HasValue) entity.OpeningBalance = dto.OpeningBalance.Value;
        entity.ClosingBalance = dto.ClosingBalance;
        // Status not mapped (FIX-5)
        entity.Notes = dto.Notes;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
