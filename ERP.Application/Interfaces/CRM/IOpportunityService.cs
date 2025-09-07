using ERP.Shared.DTOs.CRM;

namespace ERP.Application.Interfaces.CRM;

public interface IOpportunityService
{
    Task<OpportunityDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<OpportunityDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateOpportunityDto dto);
    Task<bool> UpdateAsync(UpdateOpportunityDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<OpportunityDto>> GetByStatusAsync(string status);
    Task<IEnumerable<OpportunityDto>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<OpportunityDto>> GetByStageAsync(string stage);
    Task<IEnumerable<OpportunityDto>> GetByAssignedToAsync(Guid assignedTo);
    Task<bool> ConvertToQuoteAsync(Guid opportunityId);
    Task<bool> UpdateStageAsync(Guid id, string stage, decimal probability);
    Task<bool> CloseAsync(Guid id, string reason);
}