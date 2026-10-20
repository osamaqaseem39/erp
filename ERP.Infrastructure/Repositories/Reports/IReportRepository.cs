using ERP.Domain.Reports;

namespace ERP.Infrastructure.Repositories.Reports;

public interface IReportRepository
{
    Task<Report?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Report>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<Report>> GetByTypeAsync(string reportType, Guid tenantId);
    Task<IEnumerable<Report>> GetByCategoryAsync(string category, Guid tenantId);
    Task<IEnumerable<Report>> GetActiveAsync(Guid tenantId);
    Task<Report?> GetByCodeAsync(string reportCode, Guid tenantId);
    Task<Guid> CreateAsync(Report report);
    Task<bool> UpdateAsync(Report report);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
