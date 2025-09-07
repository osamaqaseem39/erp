using ERP.Domain.HRM;

namespace ERP.Infrastructure.Repositories.HRM;

public interface IPerformanceReviewRepository
{
    Task<PerformanceReview?> GetByIdAsync(Guid id);
    Task<IEnumerable<PerformanceReview>> GetAllAsync();
    Task<IEnumerable<PerformanceReview>> GetByEmployeeAsync(Guid employeeId);
    Task<IEnumerable<PerformanceReview>> GetByReviewerAsync(Guid reviewerId);
    Task<IEnumerable<PerformanceReview>> GetByStatusAsync(string status);
    Task<Guid> CreateAsync(PerformanceReview performanceReview);
    Task<bool> UpdateAsync(PerformanceReview performanceReview);
    Task<bool> DeleteAsync(Guid id);
}