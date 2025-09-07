using ERP.Domain.HRM;

namespace ERP.Infrastructure.Interfaces.HRM;

public interface IJobApplicationRepository
{
    Task<JobApplication?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<JobApplication>> GetAllAsync(Guid tenantId);
    Task<JobApplication> CreateAsync(JobApplication entity);
    Task<JobApplication> UpdateAsync(JobApplication entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<JobApplication>> GetByJobOpeningAsync(Guid jobOpeningId, Guid tenantId);
    Task<IEnumerable<JobApplication>> GetByCandidateAsync(Guid candidateId, Guid tenantId);
    Task<IEnumerable<JobApplication>> GetByStatusAsync(string status, Guid tenantId);
}