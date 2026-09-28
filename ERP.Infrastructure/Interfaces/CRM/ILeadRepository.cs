using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface ILeadRepository
{
    Task<Lead?> GetByIdAsync(Guid id);
    Task<IEnumerable<Lead>> GetAllAsync();
    Task<IEnumerable<Lead>> GetBySourceAsync(string source);
    Task<IEnumerable<Lead>> GetByStatusAsync(string status);
    Task<IEnumerable<Lead>> GetByAssignedToAsync(Guid assignedTo);
    Task<IEnumerable<Lead>> SearchAsync(string searchTerm);
    Task<Guid> CreateAsync(Lead lead);
    Task<bool> UpdateAsync(Lead lead);
    Task<bool> DeleteAsync(Guid id);
}
