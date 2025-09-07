using ERP.Shared.DTOs.CRM;

namespace ERP.Api.Services;

public interface IActivityApiService : IApiService<ActivityDto, CreateActivityDto, UpdateActivityDto>
{
    Task<IEnumerable<ActivityDto>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<ActivityDto>> GetByLeadAsync(Guid leadId);
    Task<IEnumerable<ActivityDto>> GetByOpportunityAsync(Guid opportunityId);
    Task<IEnumerable<ActivityDto>> GetByAssignedToAsync(Guid assignedTo);
    Task<IEnumerable<ActivityDto>> GetByTypeAsync(string activityType);
    Task<IEnumerable<ActivityDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}