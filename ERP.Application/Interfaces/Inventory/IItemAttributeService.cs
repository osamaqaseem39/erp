using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Interfaces.Inventory;

public interface IItemAttributeService
{
    Task<ItemAttributeDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ItemAttributeDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateItemAttributeDto dto);
    Task<bool> UpdateAsync(UpdateItemAttributeDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<ItemAttributeDto>> GetByItemAsync(Guid itemId);
    Task<IEnumerable<ItemAttributeDto>> GetActiveAsync();
}