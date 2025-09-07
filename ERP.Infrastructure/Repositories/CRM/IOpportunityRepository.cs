using ERP.Domain.CRM;

namespace ERP.Infrastructure.Repositories.CRM;

public interface IOpportunityRepository
{
    Task<Opportunity?> GetByIdAsync(Guid id);
    Task<IEnumerable<Opportunity>> GetAllAsync();
    Task<IEnumerable<Opportunity>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<Opportunity>> GetByStatusAsync(string status);
    Task<IEnumerable<Opportunity>> GetByStageAsync(string stage);
    Task<Guid> CreateAsync(Opportunity opportunity);
    Task<bool> UpdateAsync(Opportunity opportunity);
    Task<bool> DeleteAsync(Guid id);
}