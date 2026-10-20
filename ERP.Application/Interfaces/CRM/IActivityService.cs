using ERP.Shared.DTOs.CRM;

namespace ERP.Application.Interfaces.CRM;

public interface IActivityService
{
    Task<ActivityDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ActivityDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateActivityDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateActivityDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ActivityDto>> GetByCustomerAsync(Guid customerId, Guid tenantId);
    Task<IEnumerable<ActivityDto>> GetByLeadAsync(Guid leadId, Guid tenantId);
    Task<IEnumerable<ActivityDto>> GetByOpportunityAsync(Guid opportunityId, Guid tenantId);
    Task<IEnumerable<ActivityDto>> GetByEntityAsync(string entityType, Guid entityId, Guid tenantId);
    Task<IEnumerable<ActivityDto>> GetByAssignedToAsync(Guid assignedTo, Guid tenantId);
    Task<IEnumerable<ActivityDto>> GetByTypeAsync(string activityType, Guid tenantId);
    Task<IEnumerable<ActivityDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
    Task<bool> CompleteAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> RescheduleAsync(Guid id, DateTime newDate, Guid tenantId, Guid userId);
}
