using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface IOpportunityRepository
{
    Task<Opportunity?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Opportunity>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(Opportunity opportunity);
    Task<bool> UpdateAsync(Opportunity opportunity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> ExistsAsync(Guid id, Guid tenantId);
    Task<Opportunity?> GetByCodeAsync(string code, Guid tenantId);
    Task<bool> IsCodeUniqueAsync(string code, Guid tenantId, Guid? excludeId = null);
    Task<IEnumerable<Opportunity>> GetByCustomerAsync(Guid customerId, Guid tenantId);
    Task<IEnumerable<Opportunity>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<Opportunity>> GetByAssignedToAsync(Guid assignedTo, Guid tenantId);
    Task<IEnumerable<Opportunity>> GetByStageAsync(string stage, Guid tenantId);
}
