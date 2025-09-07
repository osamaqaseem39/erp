using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface IOpportunityRepository
{
    Task<Opportunity?> GetByIdAsync(Guid id);
    Task<IEnumerable<Opportunity>> GetAllAsync();
    Task<Guid> CreateAsync(Opportunity opportunity);
    Task<bool> UpdateAsync(Opportunity opportunity);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Opportunity?> GetByCodeAsync(string code);
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);
    Task<IEnumerable<Opportunity>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<Opportunity>> GetByStatusAsync(string status);
    Task<IEnumerable<Opportunity>> GetByAssignedToAsync(Guid assignedTo);
    Task<IEnumerable<Opportunity>> GetByStageAsync(string stage);
}