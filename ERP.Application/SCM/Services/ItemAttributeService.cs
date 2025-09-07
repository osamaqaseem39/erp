using ERP.Application.SCM.Interfaces;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Shared.DTOs.Inventory;
using ERP.Shared.Mappers.Inventory;

namespace ERP.Application.SCM.Services;

public class ItemAttributeService : IItemAttributeService
{
    private readonly IItemAttributeRepository _repository;
    
    public ItemAttributeService(IItemAttributeRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ItemAttributeDto>> GetAllAsync(Guid tenantId, bool? isActive = null)
    {
        var entities = await _repository.GetAllAsync(tenantId, isActive);
        return entities.Select(ItemAttributeMapper.ToDto);
    }

    public async Task<ItemAttributeDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity != null ? ItemAttributeMapper.ToDto(entity) : null;
    }

    public async Task<ItemAttributeDto> CreateAsync(ItemAttributeDto dto, Guid tenantId, string createdBy)
    {
        var entity = ItemAttributeMapper.ToEntity(dto);
        entity.TenantId = tenantId;
        entity.CreatedBy = createdBy;
        
        var created = await _repository.CreateAsync(entity);
        return ItemAttributeMapper.ToDto(created);
    }

    public async Task<ItemAttributeDto> UpdateAsync(ItemAttributeDto dto, Guid tenantId, string updatedBy)
    {
        var entity = ItemAttributeMapper.ToEntity(dto);
        entity.TenantId = tenantId;
        entity.UpdatedBy = updatedBy;
        
        var updated = await _repository.UpdateAsync(entity);
        return ItemAttributeMapper.ToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        return await _repository.DeleteAsync(id, tenantId);
    }
}