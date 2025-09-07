using ERP.Application.SCM.Interfaces;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Shared.DTOs.Inventory;
using ERP.Shared.Mappers.Inventory;

namespace ERP.Application.SCM.Services;

public class InventoryValuationMethodService : IInventoryValuationMethodService
{
    private readonly IInventoryValuationMethodRepository _repository;
    
    public InventoryValuationMethodService(IInventoryValuationMethodRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<InventoryValuationMethodDto>> GetAllAsync(Guid tenantId, bool? isActive = null)
    {
        var entities = await _repository.GetAllAsync(tenantId, isActive);
        return entities.Select(InventoryValuationMethodMapper.ToDto);
    }

    public async Task<InventoryValuationMethodDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity != null ? InventoryValuationMethodMapper.ToDto(entity) : null;
    }

    public async Task<InventoryValuationMethodDto> CreateAsync(InventoryValuationMethodDto dto, Guid tenantId, string createdBy)
    {
        var entity = InventoryValuationMethodMapper.ToEntity(dto);
        entity.TenantId = tenantId;
        entity.CreatedBy = createdBy;
        
        var created = await _repository.CreateAsync(entity);
        return InventoryValuationMethodMapper.ToDto(created);
    }

    public async Task<InventoryValuationMethodDto> UpdateAsync(InventoryValuationMethodDto dto, Guid tenantId, string updatedBy)
    {
        var entity = InventoryValuationMethodMapper.ToEntity(dto);
        entity.TenantId = tenantId;
        entity.UpdatedBy = updatedBy;
        
        var updated = await _repository.UpdateAsync(entity);
        return InventoryValuationMethodMapper.ToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        return await _repository.DeleteAsync(id, tenantId);
    }
}