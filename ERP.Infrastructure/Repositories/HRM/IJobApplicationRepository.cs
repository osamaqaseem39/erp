using ERP.Domain.HRM;

namespace ERP.Infrastructure.Repositories.HRM;

public interface IJobApplicationRepository
{
    Task<JobApplication?> GetByIdAsync(Guid id);
    Task<IEnumerable<JobApplication>> GetAllAsync();
    Task<IEnumerable<JobApplication>> GetByCandidateAsync(Guid candidateId);
    Task<IEnumerable<JobApplication>> GetByJobOpeningAsync(Guid jobOpeningId);
    Task<IEnumerable<JobApplication>> GetByStatusAsync(string status);
    Task<Guid> CreateAsync(JobApplication jobApplication);
    Task<bool> UpdateAsync(JobApplication jobApplication);
    Task<bool> DeleteAsync(Guid id);
}