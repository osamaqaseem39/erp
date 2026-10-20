using ERP.Application.SCM.Interfaces;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Shared.DTOs.Inventory;
using ERP.Shared.Mappers.Inventory;

namespace ERP.Application.SCM.Services;

public class ItemGroupService : IItemGroupService
{
    private readonly IItemGroupRepository _repository;

    public ItemGroupService(IItemGroupRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ItemGroupDto>> GetAllAsync(Guid tenantId, bool? isActive = null)
    {
        if (tenantId == Guid.Empty)
            return Array.Empty<ItemGroupDto>();

        var entities = await _repository.GetAllAsync(tenantId, isActive);
        return entities.Select(ItemGroupMapper.ToDto);
    }

    public async Task<ItemGroupDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return null;

        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity != null ? ItemGroupMapper.ToDto(entity) : null;
    }

    public async Task<ItemGroupDto> CreateAsync(ItemGroupDto dto, Guid tenantId, string createdBy)
    {
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Tenant is required.");

        var entity = ItemGroupMapper.ToEntity(dto);
        entity.TenantId = tenantId;
        entity.CreatedBy = createdBy;

        var created = await _repository.CreateAsync(entity);
        return ItemGroupMapper.ToDto(created);
    }

    public async Task<ItemGroupDto> UpdateAsync(ItemGroupDto dto, Guid tenantId, string updatedBy)
    {
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Tenant is required.");

        var existing = await _repository.GetByIdAsync(dto.Id, tenantId)
            ?? throw new KeyNotFoundException($"Item group '{dto.Id}' not found.");

        existing.GroupCode = dto.GroupCode;
        existing.GroupName = dto.GroupName;
        existing.ParentGroupId = dto.ParentGroupId;
        existing.Description = dto.Description;
        existing.IsActive = dto.IsActive;
        existing.UpdatedBy = updatedBy;

        var updated = await _repository.UpdateAsync(existing);
        return ItemGroupMapper.ToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return false;

        return await _repository.DeleteAsync(id, tenantId);
    }
}
