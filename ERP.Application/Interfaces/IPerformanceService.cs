using ERP.Shared.DTOs.HRM;

namespace ERP.Application.Interfaces;

public interface IPerformanceService
{
    Task<PerformanceReviewDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<PerformanceReviewDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreatePerformanceReviewDto dto);
    Task<bool> UpdateAsync(UpdatePerformanceReviewDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<PerformanceReviewDto>> GetByEmployeeAsync(Guid employeeId);
    Task<IEnumerable<PerformanceReviewDto>> GetByReviewerAsync(Guid reviewerId);
    Task<IEnumerable<PerformanceReviewDto>> GetByPeriodAsync(DateTime startDate, DateTime endDate);
}