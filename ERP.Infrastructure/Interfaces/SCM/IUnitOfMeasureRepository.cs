using ERP.Domain.SCM;

namespace ERP.Infrastructure.Interfaces.SCM;

public interface IUnitOfMeasureRepository
{
    Task<IEnumerable<UnitOfMeasure>> GetAllAsync(Guid tenantId, bool? isActive = null);
    Task<UnitOfMeasure?> GetByIdAsync(Guid id, Guid tenantId);
    Task<UnitOfMeasure> CreateAsync(UnitOfMeasure entity);
    Task<UnitOfMeasure> UpdateAsync(UnitOfMeasure entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}