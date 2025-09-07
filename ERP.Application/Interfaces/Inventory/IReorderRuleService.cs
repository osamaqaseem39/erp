using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Interfaces.Inventory;

public interface IReorderRuleService
{
    Task<ReorderRuleDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ReorderRuleDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateReorderRuleDto dto);
    Task<bool> UpdateAsync(UpdateReorderRuleDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<ReorderRuleDto>> GetByItemAsync(Guid itemId);
    Task<IEnumerable<ReorderRuleDto>> GetActiveAsync();
}