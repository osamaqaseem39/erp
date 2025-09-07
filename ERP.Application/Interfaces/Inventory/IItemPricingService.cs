using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Interfaces.Inventory;

public interface IItemPricingService
{
    Task<ItemPricingDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ItemPricingDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateItemPricingDto dto);
    Task<bool> UpdateAsync(UpdateItemPricingDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<ItemPricingDto>> GetByItemAsync(Guid itemId);
    Task<ItemPricingDto?> GetCurrentPriceAsync(Guid itemId);
}