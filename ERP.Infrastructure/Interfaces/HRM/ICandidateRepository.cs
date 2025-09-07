using ERP.Domain.HRM;

namespace ERP.Infrastructure.Interfaces.HRM;

public interface ICandidateRepository
{
    Task<Candidate?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Candidate>> GetAllAsync(Guid tenantId);
    Task<Candidate> CreateAsync(Candidate entity);
    Task<Candidate> UpdateAsync(Candidate entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<Candidate?> GetByEmailAsync(string email, Guid tenantId);
    Task<IEnumerable<Candidate>> SearchAsync(string searchTerm, Guid tenantId);
    Task<IEnumerable<Candidate>> GetByStatusAsync(string status, Guid tenantId);
}