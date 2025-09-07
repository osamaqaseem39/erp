using ERP.Domain.HRM;

namespace ERP.Infrastructure.Repositories.HRM;

public interface IInterviewRepository
{
    Task<Interview?> GetByIdAsync(Guid id);
    Task<IEnumerable<Interview>> GetAllAsync();
    Task<IEnumerable<Interview>> GetByCandidateAsync(Guid candidateId);
    Task<IEnumerable<Interview>> GetByInterviewerAsync(Guid interviewerId);
    Task<IEnumerable<Interview>> GetByStatusAsync(string status);
    Task<Guid> CreateAsync(Interview interview);
    Task<bool> UpdateAsync(Interview interview);
    Task<bool> DeleteAsync(Guid id);
}