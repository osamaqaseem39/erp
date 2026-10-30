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
    private readonly ILeadRepository? _leads;

    public ActivityService(IActivityRepository repository, ILeadRepository? leads = null)
    {
        _repository = repository;
        _leads = leads;
    }

    public async Task<ActivityDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<ActivityDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetAllAsync(tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateActivityDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        var entity = MapToEntity(dto);
        entity.TenantId = tenantId;
        // ERP-FIX-5: always start Open
        entity.Status = "Open";
        entity.CreatedBy = userId != Guid.Empty ? userId.ToString() : dto.CreatedBy;
        var id = await _repository.CreateAsync(entity);
        await TouchLeadAsync(entity.LeadId, entity.StartDate, tenantId);
        return id;
    }

    public async Task<bool> UpdateAsync(UpdateActivityDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        if (!IsOpen(entity.Status)) return false;

        var previousStatus = entity.Status;
        MapToEntity(dto, entity);
        // ERP-FIX-5: status only via complete/cancel
        entity.Status = previousStatus;
        entity.UpdatedBy = userId != Guid.Empty ? userId.ToString() : dto.UpdatedBy;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!IsOpen(entity.Status)) return false;
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<IEnumerable<ActivityDto>> GetByCustomerAsync(Guid customerId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByCustomerAsync(customerId, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetByLeadAsync(Guid leadId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByLeadAsync(leadId, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetByOpportunityAsync(Guid opportunityId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByOpportunityAsync(opportunityId, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetByEntityAsync(string entityType, Guid entityId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByEntityAsync(entityType, entityId, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetByAssignedToAsync(Guid assignedTo, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByAssignedToAsync(assignedTo, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetByTypeAsync(string activityType, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByTypeAsync(activityType, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByDateRangeAsync(startDate, endDate, tenantId);
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
        var ok = await _repository.UpdateAsync(entity);
        if (ok) await TouchLeadAsync(entity.LeadId, entity.EndDate.Value, tenantId);
        return ok;
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
        return await _repository.GetByIdAsync(id, tenantId);
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

    /// <summary>Logging or completing an activity counts as contact: move the lead's last contact date forward and rescore.</summary>
    private async Task TouchLeadAsync(Guid? leadId, DateTime when, Guid tenantId)
    {
        if (_leads == null || leadId is not { } id || when == default) return;
        var now = DateTime.UtcNow;
        if (when > now) return; // a future meeting is not contact yet; completing it will count
        var lead = await _leads.GetByIdAsync(id, tenantId);
        if (lead == null || (lead.LastContactDate is { } last && last >= when)) return;
        lead.LastContactDate = when;
        lead.LeadScore = LeadScoring.Score(lead, now);
        lead.UpdatedAt = now;
        await _leads.UpdateAsync(lead);
    }
}
