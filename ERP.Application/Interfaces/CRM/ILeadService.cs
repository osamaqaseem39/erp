using ERP.Shared.DTOs.CRM;

namespace ERP.Application.Interfaces.CRM;

public interface ILeadService
{
    Task<LeadDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<LeadDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateLeadDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateLeadDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<LeadDto>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<LeadDto>> GetBySourceAsync(string source, Guid tenantId);
    Task<IEnumerable<LeadDto>> GetByAssignedToAsync(Guid assignedTo, Guid tenantId);
    Task<IEnumerable<LeadDto>> SearchAsync(string searchTerm, Guid tenantId);
    Task<bool> QualifyAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> MarkLostAsync(Guid id, Guid tenantId, Guid userId);
    Task<Guid> ConvertToOpportunityAsync(Guid leadId, Guid tenantId, Guid userId);
    Task<Guid> ConvertToCustomerAsync(Guid leadId, Guid tenantId, Guid userId);
}
