using ERP.Shared.DTOs.CRM;

namespace ERP.Api.Services;

public interface ILeadApiService : IApiService<LeadDto, CreateLeadDto, UpdateLeadDto>
{
    Task<IEnumerable<LeadDto>> GetBySourceAsync(string source);
    Task<IEnumerable<LeadDto>> GetByStatusAsync(string status);
    Task<IEnumerable<LeadDto>> GetByAssignedToAsync(Guid assignedTo);
    Task<Guid> ConvertToCustomerAsync(Guid id);
}