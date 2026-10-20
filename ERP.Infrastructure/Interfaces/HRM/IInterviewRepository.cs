using ERP.Domain.HRM;

namespace ERP.Infrastructure.Interfaces.HRM;

public interface IInterviewRepository
{
    Task<Interview?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Interview>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(Interview entity);
    Task<bool> UpdateAsync(Interview entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Interview>> GetByJobApplicationAsync(Guid jobApplicationId, Guid tenantId);
    Task<IEnumerable<Interview>> GetByInterviewerAsync(Guid interviewerId, Guid tenantId);
    Task<IEnumerable<Interview>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
    Task<IEnumerable<Interview>> GetByCandidateAsync(Guid candidateId, Guid tenantId);
}
