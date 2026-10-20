using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface IActivityRepository
{
    Task<Activity?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Activity>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(Activity activity);
    Task<bool> UpdateAsync(Activity activity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> ExistsAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Activity>> GetByCustomerAsync(Guid customerId, Guid tenantId);
    Task<IEnumerable<Activity>> GetByLeadAsync(Guid leadId, Guid tenantId);
    Task<IEnumerable<Activity>> GetByOpportunityAsync(Guid opportunityId, Guid tenantId);
    Task<IEnumerable<Activity>> GetByAssignedToAsync(Guid assignedTo, Guid tenantId);
    Task<IEnumerable<Activity>> GetByTypeAsync(string activityType, Guid tenantId);
    Task<IEnumerable<Activity>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
    Task<IEnumerable<Activity>> GetByEntityAsync(string entityType, Guid entityId, Guid tenantId);
}
