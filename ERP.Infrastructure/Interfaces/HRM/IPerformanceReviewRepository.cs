using ERP.Domain.HRM;

namespace ERP.Infrastructure.Interfaces.HRM;

public interface IPerformanceReviewRepository
{
    Task<PerformanceReview?> GetByIdAsync(Guid id);
    Task<IEnumerable<PerformanceReview>> GetAllAsync();
    Task<Guid> CreateAsync(PerformanceReview performanceReview);
    Task<bool> UpdateAsync(PerformanceReview performanceReview);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<PerformanceReview>> GetByEmployeeAsync(Guid employeeId);
    Task<IEnumerable<PerformanceReview>> GetByReviewerAsync(Guid reviewerId);
    Task<IEnumerable<PerformanceReview>> GetByPeriodAsync(DateTime startDate, DateTime endDate);
}