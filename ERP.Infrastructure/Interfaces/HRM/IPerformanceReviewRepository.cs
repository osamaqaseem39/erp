using ERP.Domain.HRM;

namespace ERP.Infrastructure.Interfaces.HRM;

public interface IPerformanceReviewRepository
{
    Task<PerformanceReview?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PerformanceReview>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(PerformanceReview performanceReview);
    Task<bool> UpdateAsync(PerformanceReview performanceReview);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> ExistsAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PerformanceReview>> GetByEmployeeAsync(Guid employeeId, Guid tenantId);
    Task<IEnumerable<PerformanceReview>> GetByReviewerAsync(Guid reviewerId, Guid tenantId);
    Task<IEnumerable<PerformanceReview>> GetByPeriodAsync(DateTime startDate, DateTime endDate, Guid tenantId);
}
