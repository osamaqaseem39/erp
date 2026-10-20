using ERP.Shared.DTOs.CRM;

namespace ERP.Application.Interfaces.CRM;

public interface IOpportunityService
{
    Task<OpportunityDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<OpportunityDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateOpportunityDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateOpportunityDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<OpportunityDto>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<OpportunityDto>> GetByCustomerAsync(Guid customerId, Guid tenantId);
    Task<IEnumerable<OpportunityDto>> GetByStageAsync(string stage, Guid tenantId);
    Task<IEnumerable<OpportunityDto>> GetByAssignedToAsync(Guid assignedTo, Guid tenantId);
    Task<bool> MarkWonAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> MarkLostAsync(Guid id, Guid tenantId, Guid userId, string? reason = null);
    Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId);
    Task<Guid> ConvertToQuoteAsync(Guid opportunityId, Guid tenantId, Guid userId);
    Task<bool> UpdateStageAsync(Guid id, string stage, decimal probability, Guid tenantId);
    Task<bool> CloseAsync(Guid id, Guid tenantId, Guid userId, string? reason = null);
}
