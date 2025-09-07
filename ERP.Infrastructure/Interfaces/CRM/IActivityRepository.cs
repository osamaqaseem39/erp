using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface IActivityRepository
{
    Task<Activity?> GetByIdAsync(Guid id);
    Task<IEnumerable<Activity>> GetAllAsync();
    Task<Guid> CreateAsync(Activity activity);
    Task<bool> UpdateAsync(Activity activity);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<Activity>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<Activity>> GetByLeadAsync(Guid leadId);
    Task<IEnumerable<Activity>> GetByOpportunityAsync(Guid opportunityId);
    Task<IEnumerable<Activity>> GetByAssignedToAsync(Guid assignedTo);
    Task<IEnumerable<Activity>> GetByTypeAsync(string activityType);
    Task<IEnumerable<Activity>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Activity>> GetByEntityAsync(string entityType, Guid entityId);
}