using ERP.Application.SCM.Interfaces;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Shared.DTOs.Inventory;
using ERP.Shared.Mappers.Inventory;

namespace ERP.Application.SCM.Services;

public class ItemVariantService : IItemVariantService
{
    private readonly IItemVariantRepository _repository;
    
    public ItemVariantService(IItemVariantRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ItemVariantDto>> GetAllAsync(Guid tenantId)
    {
        var entities = await _repository.GetAllAsync(tenantId);
        return entities.Select(ItemVariantMapper.ToDto);
    }

    public async Task<IEnumerable<ItemVariantDto>> GetByItemAsync(Guid itemId, Guid tenantId)
    {
        var entities = await _repository.GetByItemAsync(itemId, tenantId);
        return entities.Select(ItemVariantMapper.ToDto);
    }

    public async Task<ItemVariantDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity != null ? ItemVariantMapper.ToDto(entity) : null;
    }

    public async Task<ItemVariantDto> CreateAsync(ItemVariantDto dto, Guid tenantId, string createdBy)
    {
        var entity = ItemVariantMapper.ToEntity(dto);
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        entity.TenantId = tenantId;
        entity.CreatedBy = createdBy;
        
        var created = await _repository.CreateAsync(entity);
        return ItemVariantMapper.ToDto(created);
    }

    public async Task<ItemVariantDto> UpdateAsync(ItemVariantDto dto, Guid tenantId, string updatedBy)
    {
        var existing = await _repository.GetByIdAsync(dto.Id, tenantId)
            ?? throw new KeyNotFoundException($"Item variant '{dto.Id}' not found.");

        existing.ItemId = dto.ItemId;
        existing.VariantCode = dto.VariantCode;
        existing.VariantName = dto.VariantName;
        existing.SKU = dto.SKU;
        existing.CostPrice = dto.CostPrice;
        existing.SellingPrice = dto.SellingPrice;
        existing.IsActive = dto.IsActive;
        existing.UpdatedBy = updatedBy;

        var updated = await _repository.UpdateAsync(existing);
        return ItemVariantMapper.ToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        return await _repository.DeleteAsync(id, tenantId);
    }
}