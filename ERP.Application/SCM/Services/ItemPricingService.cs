using ERP.Application.SCM.Interfaces;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Shared.DTOs.Inventory;
using ERP.Shared.Mappers.Inventory;

namespace ERP.Application.SCM.Services;

public class ItemPricingService : IItemPricingService
{
    private readonly IItemPricingRepository _repository;
    public ItemPricingService(IItemPricingRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ItemPricingDto>> GetByItemAsync(Guid itemId, Guid tenantId, string? priceListName = null)
    {
        var entities = await _repository.GetByItemAsync(itemId, tenantId, priceListName);
        var mapper = new ItemPricingMapper();
        return entities.Select(mapper.ToDto);
    }

    public async Task<ItemPricingDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        var mapper = new ItemPricingMapper();
        return entity != null ? mapper.ToDto(entity) : null;
    }

    public async Task<ItemPricingDto> CreateAsync(ItemPricingDto dto, Guid tenantId, string createdBy)
    {
        var mapper = new ItemPricingMapper();
        var entity = mapper.ToEntity(dto);
        entity.TenantId = tenantId;
        entity.CreatedBy = createdBy;
        
        var created = await _repository.CreateAsync(entity);
        return mapper.ToDto(created);
    }

    public async Task<ItemPricingDto> UpdateAsync(ItemPricingDto dto, Guid tenantId, string updatedBy)
    {
        var mapper = new ItemPricingMapper();
        var entity = mapper.ToEntity(dto);
        entity.TenantId = tenantId;
        entity.UpdatedBy = updatedBy;
        
        var updated = await _repository.UpdateAsync(entity);
        return mapper.ToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        return await _repository.DeleteAsync(id, tenantId);
    }
}