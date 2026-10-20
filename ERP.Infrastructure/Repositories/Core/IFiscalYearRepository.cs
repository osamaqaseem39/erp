using ERP.Domain.Core;

namespace ERP.Infrastructure.Repositories.Core;

public interface IFiscalYearRepository
{
    Task<FiscalYear?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<FiscalYear>> GetAllAsync(Guid tenantId, string? status = null);
    Task<FiscalYear?> FindActiveCoveringAsync(Guid tenantId, DateTime date);
    Task<bool> HasAnyActiveAsync(Guid tenantId);
    Task<bool> HasOverlapAsync(Guid tenantId, DateTime start, DateTime end, Guid? excludeId = null);
    Task<Guid> CreateAsync(FiscalYear entity);
    Task<bool> UpdateAsync(FiscalYear entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
