using ERP.Shared.DTOs.CRM;

namespace ERP.Application.Interfaces.CRM;

public interface ILeadService
{
    Task<LeadDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<LeadDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateLeadDto dto);
    Task<bool> UpdateAsync(UpdateLeadDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<LeadDto>> GetByStatusAsync(string status);
    Task<IEnumerable<LeadDto>> GetBySourceAsync(string source);
    Task<IEnumerable<LeadDto>> GetByAssignedToAsync(Guid assignedTo);
    Task<IEnumerable<LeadDto>> SearchAsync(string searchTerm);
    Task<Guid> ConvertToOpportunityAsync(Guid leadId);
    Task<Guid> ConvertToCustomerAsync(Guid leadId);
}