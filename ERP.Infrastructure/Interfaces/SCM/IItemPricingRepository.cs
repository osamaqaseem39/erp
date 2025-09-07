using ERP.Domain.SCM;

namespace ERP.Infrastructure.Interfaces.SCM;

public interface IItemPricingRepository
{
    Task<IEnumerable<ItemPricing>> GetByItemAsync(Guid itemId, Guid tenantId, string? priceListName = null);
    Task<ItemPricing?> GetByIdAsync(Guid id, Guid tenantId);
    Task<ItemPricing> CreateAsync(ItemPricing entity);
    Task<ItemPricing> UpdateAsync(ItemPricing entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}