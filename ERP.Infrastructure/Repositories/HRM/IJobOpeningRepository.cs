using ERP.Domain.HRM;

namespace ERP.Infrastructure.Repositories.HRM;

public interface IJobOpeningRepository
{
    Task<JobOpening?> GetByIdAsync(Guid id);
    Task<IEnumerable<JobOpening>> GetAllAsync();
    Task<IEnumerable<JobOpening>> GetByStatusAsync(string status);
    Task<IEnumerable<JobOpening>> GetByDepartmentAsync(string department);
    Task<IEnumerable<JobOpening>> GetByLocationAsync(string location);
    Task<Guid> CreateAsync(JobOpening jobOpening);
    Task<bool> UpdateAsync(JobOpening jobOpening);
    Task<bool> DeleteAsync(Guid id);
}