using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Interfaces.Inventory;

public interface IItemGroupService
{
    Task<ItemGroupDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ItemGroupDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateItemGroupDto dto);
    Task<bool> UpdateAsync(UpdateItemGroupDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<ItemGroupDto>> GetActiveAsync();
}