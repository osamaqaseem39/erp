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
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        entity.TenantId = tenantId;
        entity.CreatedBy = createdBy;

        var created = await _repository.CreateAsync(entity);
        return _mapper.ToDto(created);
    }

    public async Task<ReorderRuleDto> UpdateAsync(ReorderRuleDto dto, Guid tenantId, string updatedBy)
    {
        var existing = await _repository.GetByIdAsync(dto.Id, tenantId)
            ?? throw new KeyNotFoundException($"Reorder rule '{dto.Id}' not found.");

        existing.ItemId = dto.ItemId;
        existing.WarehouseId = dto.WarehouseId;
        existing.MinStockLevel = dto.MinStockLevel;
        existing.MaxStockLevel = dto.MaxStockLevel;
        existing.ReorderPoint = dto.ReorderPoint;
        existing.ReorderQuantity = dto.ReorderQuantity;
        existing.LeadTimeDays = dto.LeadTimeDays;
        existing.IsActive = dto.IsActive;
        existing.UpdatedBy = updatedBy;

        var updated = await _repository.UpdateAsync(existing);
        return _mapper.ToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        return await _repository.DeleteAsync(id, tenantId);
    }
}
