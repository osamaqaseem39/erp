using ERP.Domain.Core;

namespace ERP.Infrastructure.Repositories.Core;

public interface IPlanRepository
{
    Task<Plan?> GetByIdAsync(Guid id);
    Task<IEnumerable<Plan>> GetAllAsync();
    Task<IEnumerable<Plan>> GetActiveAsync();
    Task<Plan?> GetByNameAsync(string planName);
    Task<Guid> CreateAsync(Plan plan);
    Task<bool> UpdateAsync(Plan plan);
    Task<bool> DeleteAsync(Guid id);
}