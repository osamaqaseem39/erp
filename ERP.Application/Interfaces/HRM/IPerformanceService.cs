using ERP.Shared.DTOs.HRM;

namespace ERP.Application.Interfaces.HRM;

public interface IPerformanceService
{
    Task<PerformanceReviewDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PerformanceReviewDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreatePerformanceReviewDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdatePerformanceReviewDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PerformanceReviewDto>> GetByEmployeeAsync(Guid employeeId, Guid tenantId);
    Task<IEnumerable<PerformanceReviewDto>> GetByReviewerAsync(Guid reviewerId, Guid tenantId);
    Task<IEnumerable<PerformanceReviewDto>> GetByPeriodAsync(DateTime startDate, DateTime endDate, Guid tenantId);
    Task<bool> SubmitAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> CompleteAsync(Guid id, Guid tenantId, Guid userId);
}
