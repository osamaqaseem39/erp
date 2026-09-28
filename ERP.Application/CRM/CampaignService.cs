using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Domain.CRM;

namespace ERP.Application.CRM;

public class CampaignService : ICampaignService
{
    private readonly ICampaignRepository _repository;

    public CampaignService(ICampaignRepository repository)
    {
        _repository = repository;
    }

    public async Task<CampaignDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<CampaignDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateCampaignDto dto)
    {
        var entity = MapToEntity(dto);
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateCampaignDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id);
        if (entity == null) return false;

        MapToEntity(dto, entity);
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<CampaignDto>> GetByStatusAsync(string status)
    {
        var entities = await _repository.GetByStatusAsync(status);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<CampaignDto>> GetByTypeAsync(string campaignType)
    {
        var entities = await _repository.GetByTypeAsync(campaignType);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<CampaignDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var entities = await _repository.GetAllAsync();
        return entities
            .Where(c => c.StartDate >= startDate && c.StartDate <= endDate)
            .Select(MapToDto);
    }

    public async Task<bool> LaunchAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        entity.Status = "Active";
        entity.LaunchDate = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> PauseAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        entity.Status = "Paused";
        entity.PausedDate = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> CompleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        entity.Status = "Completed";
        entity.CompletedDate = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
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
            CompanyId = dto.CompanyId.HasValue ? dto.CompanyId.Value : Guid.Empty,
            CampaignName = dto.CampaignName ?? string.Empty,
            Description = dto.Description,
            CampaignType = dto.CampaignType ?? string.Empty,
            Status = dto.Status ?? "Draft",
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
        if (dto.Status != null) entity.Status = dto.Status;
        if (dto.StartDate.HasValue) entity.StartDate = dto.StartDate.Value;
        entity.EndDate = dto.EndDate;
        entity.Budget = dto.Budget;
        entity.Currency = dto.Currency;
        entity.TargetAudience = dto.TargetAudience;
        entity.Goals = dto.Goals;
        entity.UpdatedBy = !string.IsNullOrEmpty(dto.UpdatedBy) ? dto.UpdatedBy : null;
    }
}