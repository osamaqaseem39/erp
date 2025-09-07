using ERP.Domain.Reports;

namespace ERP.Infrastructure.Repositories.Reports;

public interface IReportRepository
{
    Task<Report?> GetByIdAsync(Guid id);
    Task<IEnumerable<Report>> GetAllAsync();
    Task<IEnumerable<Report>> GetByTypeAsync(string reportType);
    Task<IEnumerable<Report>> GetByCategoryAsync(string category);
    Task<IEnumerable<Report>> GetActiveAsync();
    Task<Report?> GetByCodeAsync(string reportCode);
    Task<Guid> CreateAsync(Report report);
    Task<bool> UpdateAsync(Report report);
    Task<bool> DeleteAsync(Guid id);
}