using ERP.Shared.DTOs.CRM;
using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface ILeadRepository
{
    Task<Lead?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Lead>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<Lead>> GetBySourceAsync(string source, Guid tenantId);
    Task<IEnumerable<Lead>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<Lead>> GetByAssignedToAsync(Guid assignedTo, Guid tenantId);
    Task<IEnumerable<Lead>> SearchAsync(string searchTerm, Guid tenantId);
    Task<Guid> CreateAsync(Lead lead);
    Task<bool> UpdateAsync(Lead lead);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    /// <summary>Leads, customers and contacts whose email or phone matches. Phones compare on digits only.</summary>
    Task<IReadOnlyList<LeadDuplicateDto>> FindDuplicatesAsync(string? email, string? phone, Guid? excludeLeadId, Guid tenantId);
}
