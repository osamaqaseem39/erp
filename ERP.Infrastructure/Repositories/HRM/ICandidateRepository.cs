using ERP.Domain.HRM;

namespace ERP.Infrastructure.Repositories.HRM;

public interface ICandidateRepository
{
    Task<Candidate?> GetByIdAsync(Guid id);
    Task<IEnumerable<Candidate>> GetAllAsync();
    Task<IEnumerable<Candidate>> GetByStatusAsync(string status);
    Task<IEnumerable<Candidate>> GetByJobOpeningAsync(Guid jobOpeningId);
    Task<Guid> CreateAsync(Candidate candidate);
    Task<bool> UpdateAsync(Candidate candidate);
    Task<bool> DeleteAsync(Guid id);
}