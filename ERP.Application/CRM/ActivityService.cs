using ERP.Application.Common;
using ERP.Application.Interfaces.CRM;
using ERP.Domain.CRM;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;

namespace ERP.Application.CRM;

public class ActivityService : IActivityService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Open"] = new[] { "Completed", "Cancelled" },
            ["Completed"] = Array.Empty<string>(),
            ["Cancelled"] = Array.Empty<string>()
        };

    private readonly IActivityRepository _repository;

    public ActivityService(IActivityRepository repository)
    {
        _repository = repository;
    }

    public async Task<ActivityDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<ActivityDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateActivityDto dto)
    {
        var entity = MapToEntity(dto);
        // ERP-FIX-5: always start Open
        entity.Status = "Open";
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateActivityDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id);
        if (entity == null) return false;
        if (!IsOpen(entity.Status)) return false;

        var previousStatus = entity.Status;
        MapToEntity(dto, entity);
        // ERP-FIX-5: status only via complete/cancel
        entity.Status = previousStatus;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;
        if (!IsOpen(entity.Status)) return false;
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<ActivityDto>> GetByCustomerAsync(Guid customerId)
    {
        var entities = await _repository.GetByCustomerAsync(customerId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetByLeadAsync(Guid leadId)
    {
        var entities = await _repository.GetByLeadAsync(leadId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetByOpportunityAsync(Guid opportunityId)
    {
        var entities = await _repository.GetByOpportunityAsync(opportunityId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetByEntityAsync(string entityType, Guid entityId)
    {
        var entities = await _repository.GetByEntityAsync(entityType, entityId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetByAssignedToAsync(Guid assignedTo)
    {
        var entities = await _repository.GetByAssignedToAsync(assignedTo);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetByTypeAsync(string activityType)
    {
        var entities = await _repository.GetByTypeAsync(activityType);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var entities = await _repository.GetByDateRangeAsync(startDate, endDate);
        return entities.Select(MapToDto);
    }

    public async Task<bool> CompleteAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await GetScopedAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(StatusOf(entity), "Completed", Transitions))
            return false;

        entity.Status = "Completed";
        entity.EndDate ??= DateTime.UtcNow;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await GetScopedAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(StatusOf(entity), "Cancelled", Transitions))
            return false;

        entity.Status = "Cancelled";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> RescheduleAsync(Guid id, DateTime newDate, Guid tenantId, Guid userId)
    {
        var entity = await GetScopedAsync(id, tenantId);
        if (entity == null) return false;
        if (!IsOpen(entity.Status)) return false;

        entity.StartDate = newDate;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    private async Task<Activity?> GetScopedAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null || entity.TenantId != tenantId) return null;
        return entity;
    }

    private static bool IsOpen(string? status) =>
        string.Equals(StatusOf(status), "Open", StringComparison.OrdinalIgnoreCase);

    private static string StatusOf(Activity entity) => StatusOf(entity.Status);

    private static string StatusOf(string? status) =>
        string.IsNullOrWhiteSpace(status) ? "Open" : status.Trim();

    private static ActivityDto MapToDto(Activity entity)
    {
        return new ActivityDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            ActivityType = entity.ActivityType,
            Type = entity.ActivityType,
            Subject = entity.Subject,
            Description = entity.Description,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Status = entity.Status,
            Priority = entity.Priority,
            AssignedToUserId = entity.AssignedToUserId,
            AssignedToUserName = entity.AssignedToUser != null
                ? $"{entity.AssignedToUser.FirstName} {entity.AssignedToUser.LastName}".Trim()
                : null,
            Location = entity.Location,
            Notes = entity.Notes,
            RelatedEntityId = entity.RelatedEntityId,
            RelatedToId = entity.RelatedEntityId,
            RelatedEntityType = entity.RelatedEntityType,
            RelatedToType = entity.RelatedEntityType,
            CustomerId = entity.CustomerId,
            CustomerName = entity.Customer?.CompanyName,
            LeadId = entity.LeadId,
            LeadName = entity.Lead?.CompanyName,
            OpportunityId = entity.OpportunityId,
            OpportunityName = entity.Opportunity?.OpportunityName,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            IsActive = entity.IsActive
        };
    }

    private static Activity MapToEntity(CreateActivityDto dto)
    {
        return new Activity
        {
            TenantId = dto.TenantId,
            ActivityType = dto.ActivityType ?? string.Empty,
            Subject = dto.Subject ?? string.Empty,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = "Open",
            Priority = dto.Priority ?? "Medium",
            AssignedToUserId = dto.AssignedToUserId,
            Location = dto.Location,
            Notes = dto.Notes,
            RelatedEntityId = dto.RelatedEntityId,
            RelatedEntityType = dto.RelatedEntityType,
            CustomerId = dto.CustomerId,
            LeadId = dto.LeadId,
            OpportunityId = dto.OpportunityId,
            CreatedBy = dto.CreatedBy
        };
    }

    private static void MapToEntity(UpdateActivityDto dto, Activity entity)
    {
        entity.ActivityType = dto.ActivityType ?? entity.ActivityType;
        entity.Subject = dto.Subject ?? entity.Subject;
        entity.Description = dto.Description;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        entity.Priority = dto.Priority ?? entity.Priority;
        entity.AssignedToUserId = dto.AssignedToUserId;
        entity.Location = dto.Location;
        entity.Notes = dto.Notes;
        entity.RelatedEntityId = dto.RelatedEntityId;
        entity.RelatedEntityType = dto.RelatedEntityType;
        entity.CustomerId = dto.CustomerId;
        entity.LeadId = dto.LeadId;
        entity.OpportunityId = dto.OpportunityId;
        entity.UpdatedBy = dto.UpdatedBy;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
