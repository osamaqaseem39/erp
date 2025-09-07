using ERP.Shared.DTOs.CRM;

namespace ERP.Application.Interfaces.CRM;

public interface IActivityService
{
    Task<ActivityDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ActivityDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateActivityDto dto);
    Task<bool> UpdateAsync(UpdateActivityDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<ActivityDto>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<ActivityDto>> GetByLeadAsync(Guid leadId);
    Task<IEnumerable<ActivityDto>> GetByOpportunityAsync(Guid opportunityId);
    Task<IEnumerable<ActivityDto>> GetByAssignedToAsync(Guid assignedTo);
    Task<IEnumerable<ActivityDto>> GetByTypeAsync(string activityType);
    Task<IEnumerable<ActivityDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> CompleteAsync(Guid id);
    Task<bool> RescheduleAsync(Guid id, DateTime newDate);
}