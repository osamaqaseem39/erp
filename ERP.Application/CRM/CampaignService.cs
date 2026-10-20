using ERP.Application.Common;
using ERP.Application.Interfaces.CRM;
using ERP.Domain.CRM;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;

namespace ERP.Application.CRM;

public class CampaignService : ICampaignService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Draft"] = new[] { "Active", "Cancelled" },
            ["Active"] = new[] { "Paused", "Completed", "Cancelled" },
            ["Paused"] = new[] { "Active", "Completed", "Cancelled" },
            ["Completed"] = Array.Empty<string>(),
            ["Cancelled"] = Array.Empty<string>()
        };

    private readonly ICampaignRepository _repository;

    public CampaignService(ICampaignRepository repository)
    {
        _repository = repository;
    }

    public async Task<CampaignDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<CampaignDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetAllAsync(tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateCampaignDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        var entity = MapToEntity(dto);
        entity.TenantId = tenantId;
        // ERP-FIX-5: always start Draft
        entity.Status = "Draft";
        entity.CreatedBy = userId != Guid.Empty ? userId.ToString() : dto.CreatedBy;
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateCampaignDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(DocumentWorkflow.Normalize(entity.Status), "Draft", StringComparison.OrdinalIgnoreCase))
            return false;

        var previousStatus = entity.Status;
        MapToEntity(dto, entity);
        // ERP-FIX-5: status only via launch/pause/complete/cancel
        entity.Status = previousStatus;
        entity.UpdatedBy = userId != Guid.Empty ? userId.ToString() : dto.UpdatedBy;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(DocumentWorkflow.Normalize(entity.Status), "Draft", StringComparison.OrdinalIgnoreCase))
            return false;
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<IEnumerable<CampaignDto>> GetByStatusAsync(string status, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByStatusAsync(status, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<CampaignDto>> GetByTypeAsync(string campaignType, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByTypeAsync(campaignType, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<CampaignDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetAllAsync(tenantId);
        return entities
            .Where(c => c.StartDate >= startDate && c.StartDate <= endDate)
            .Select(MapToDto);
    }

    public async Task<bool> LaunchAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await GetScopedAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Active", Transitions))
            return false;

        entity.Status = "Active";
        entity.LaunchDate = DateTime.UtcNow;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> PauseAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await GetScopedAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Paused", Transitions))
            return false;

        entity.Status = "Paused";
        entity.PausedDate = DateTime.UtcNow;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> CompleteAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await GetScopedAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Completed", Transitions))
            return false;

        entity.Status = "Completed";
        entity.CompletedDate = DateTime.UtcNow;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await GetScopedAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Cancelled", Transitions))
            return false;

        entity.Status = "Cancelled";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    private async Task<Campaign?> GetScopedAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await _repository.GetByIdAsync(id, tenantId);
    }

    private static CampaignDto MapToDto(Campaign entity)
    {
        return new CampaignDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CompanyId = entity.CompanyId,
            CampaignName = entity.CampaignName,
            Description = entity.Description,
            CampaignType = entity.CampaignType,
            Status = entity.Status,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Budget = entity.Budget,
            Currency = entity.Currency,
            TargetAudience = entity.TargetAudience,
            Goals = entity.Goals,
            LaunchDate = entity.LaunchDate,
            PausedDate = entity.PausedDate,
            CompletedDate = entity.CompletedDate,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            IsActive = entity.IsActive
        };
    }

    private static Campaign MapToEntity(CreateCampaignDto dto)
    {
        return new Campaign
        {
            TenantId = dto.TenantId,
            CompanyId = dto.CompanyId ?? Guid.Empty,
            CampaignName = dto.CampaignName ?? string.Empty,
            Description = dto.Description,
            CampaignType = dto.CampaignType ?? string.Empty,
            Status = "Draft",
            StartDate = dto.StartDate ?? DateTime.UtcNow,
            EndDate = dto.EndDate,
            Budget = dto.Budget,
            Currency = dto.Currency,
            TargetAudience = dto.TargetAudience,
            Goals = dto.Goals,
            CreatedBy = !string.IsNullOrEmpty(dto.CreatedBy) ? dto.CreatedBy : null
        };
    }

    private static void MapToEntity(UpdateCampaignDto dto, Campaign entity)
    {
        if (dto.CampaignName != null) entity.CampaignName = dto.CampaignName;
        entity.Description = dto.Description;
        if (dto.CampaignType != null) entity.CampaignType = dto.CampaignType;
        if (dto.StartDate.HasValue) entity.StartDate = dto.StartDate.Value;
        entity.EndDate = dto.EndDate;
        entity.Budget = dto.Budget;
        entity.Currency = dto.Currency;
        entity.TargetAudience = dto.TargetAudience;
        entity.Goals = dto.Goals;
        entity.UpdatedBy = !string.IsNullOrEmpty(dto.UpdatedBy) ? dto.UpdatedBy : null;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
