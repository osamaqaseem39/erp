using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.SCM.Interfaces;

public interface IItemPricingService
{
    Task<IEnumerable<ItemPricingDto>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<ItemPricingDto>> GetByItemAsync(Guid itemId, Guid tenantId, string? priceListName = null);
    Task<ItemPricingDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<ItemPricingDto> CreateAsync(ItemPricingDto dto, Guid tenantId, string createdBy);
    Task<ItemPricingDto> UpdateAsync(ItemPricingDto dto, Guid tenantId, string updatedBy);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}