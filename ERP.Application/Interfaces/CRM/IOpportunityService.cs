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
    Task<bool> MarkWonAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> MarkLostAsync(Guid id, Guid tenantId, Guid userId, string? reason = null);
    Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId);
    Task<Guid> ConvertToQuoteAsync(Guid opportunityId, Guid tenantId, Guid userId);
    Task<bool> UpdateStageAsync(Guid id, string stage, decimal probability);
    Task<bool> CloseAsync(Guid id, Guid tenantId, Guid userId, string? reason = null);
}
