using ERP.Application.SCM.Interfaces;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Shared.DTOs.Inventory;
using ERP.Shared.Mappers.Inventory;

namespace ERP.Application.SCM.Services;

public class ReorderRuleService : IReorderRuleService
{
    private readonly IReorderRuleRepository _repository;
    private readonly ReorderRuleMapper _mapper = new();
    
    public ReorderRuleService(IReorderRuleRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ReorderRuleDto>> GetByItemAsync(Guid? itemId, Guid? warehouseId, Guid tenantId)
    {
        var entities = await _repository.GetByItemAsync(itemId, warehouseId, tenantId);
        return entities.Select(_mapper.ToDto);
    }

    public async Task<ReorderRuleDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity != null ? _mapper.ToDto(entity) : null;
    }

    public async Task<ReorderRuleDto> CreateAsync(ReorderRuleDto dto, Guid tenantId, string createdBy)
    {
        var entity = _mapper.ToEntity(dto);
        entity.TenantId = tenantId;
        entity.CreatedBy = createdBy;
        
        var created = await _repository.CreateAsync(entity);
        return _mapper.ToDto(created);
    }

    public async Task<ReorderRuleDto> UpdateAsync(ReorderRuleDto dto, Guid tenantId, string updatedBy)
    {
        var entity = _mapper.ToEntity(dto);
        entity.TenantId = tenantId;
        entity.UpdatedBy = updatedBy;
        
        var updated = await _repository.UpdateAsync(entity);
        return _mapper.ToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        return await _repository.DeleteAsync(id, tenantId);
    }
}