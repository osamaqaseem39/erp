using ERP.Shared.DTOs.CRM;

namespace ERP.Api.Services;

public interface IOpportunityApiService : IApiService<OpportunityDto, CreateOpportunityDto, UpdateOpportunityDto>
{
    Task<IEnumerable<OpportunityDto>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<OpportunityDto>> GetByStatusAsync(string status);
    Task<IEnumerable<OpportunityDto>> GetByAssignedToAsync(Guid assignedTo);
    Task<IEnumerable<OpportunityDto>> GetByStageAsync(string stage);
    Task<bool> ConvertToQuoteAsync(Guid id);
}