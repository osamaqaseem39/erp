using ERP.Domain.CRM;

namespace ERP.Infrastructure.Repositories.CRM;

public interface ILeadRepository
{
    Task<Lead?> GetByIdAsync(Guid id);
    Task<IEnumerable<Lead>> GetAllAsync();
    Task<IEnumerable<Lead>> GetByStatusAsync(string status);
    Task<IEnumerable<Lead>> GetBySourceAsync(string source);
    Task<IEnumerable<Lead>> GetByAssignedToAsync(Guid assignedToId);
    Task<Guid> CreateAsync(Lead lead);
    Task<bool> UpdateAsync(Lead lead);
    Task<bool> DeleteAsync(Guid id);
}