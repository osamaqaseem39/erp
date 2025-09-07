using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface ILeadRepository
{
    Task<Lead?> GetByIdAsync(Guid id);
    Task<IEnumerable<Lead>> GetAllAsync();
    Task<Guid> CreateAsync(Lead lead);
    Task<bool> UpdateAsync(Lead lead);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Lead?> GetByCodeAsync(string code);
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);
    Task<IEnumerable<Lead>> GetBySourceAsync(string source);
    Task<IEnumerable<Lead>> GetByStatusAsync(string status);
    Task<IEnumerable<Lead>> GetByAssignedToAsync(Guid assignedTo);
    Task<IEnumerable<Lead>> SearchAsync(string searchTerm);
}