using ERP.Shared.DTOs.Core;

namespace ERP.Application.Interfaces.Core;

public interface IPlanService
{
    Task<PlanDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<PlanDto>> GetAllAsync();
    Task<IEnumerable<PlanDto>> GetActiveAsync();
    Task<PlanDto?> GetByNameAsync(string planName);
    Task<Guid> CreateAsync(CreatePlanDto dto);
    Task<bool> UpdateAsync(UpdatePlanDto dto);
    Task<bool> DeleteAsync(Guid id);
}