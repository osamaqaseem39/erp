using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Interfaces.Inventory;

public interface IItemVariantService
{
    Task<ItemVariantDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ItemVariantDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateItemVariantDto dto);
    Task<bool> UpdateAsync(UpdateItemVariantDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<ItemVariantDto>> GetByItemAsync(Guid itemId);
    Task<IEnumerable<ItemVariantDto>> GetActiveAsync();
}