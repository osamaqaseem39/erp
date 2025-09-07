using ERP.Application.SCM.Interfaces;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Shared.DTOs.Inventory;
using ERP.Shared.Mappers.Inventory;

namespace ERP.Application.SCM.Services;

public class UnitOfMeasureService : IUnitOfMeasureService
{
    private readonly IUnitOfMeasureRepository _repository;
    
    public UnitOfMeasureService(IUnitOfMeasureRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<UnitOfMeasureDto>> GetAllAsync(Guid tenantId, bool? isActive = null)
    {
        var entities = await _repository.GetAllAsync(tenantId, isActive);
        return entities.Select(UnitOfMeasureMapper.ToDto);
    }

    public async Task<UnitOfMeasureDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity != null ? UnitOfMeasureMapper.ToDto(entity) : null;
    }

    public async Task<UnitOfMeasureDto> CreateAsync(UnitOfMeasureDto dto, Guid tenantId, string createdBy)
    {
        var entity = UnitOfMeasureMapper.ToEntity(dto);
        entity.TenantId = tenantId;
        entity.CreatedBy = createdBy;
        
        var created = await _repository.CreateAsync(entity);
        return UnitOfMeasureMapper.ToDto(created);
    }

    public async Task<UnitOfMeasureDto> UpdateAsync(UnitOfMeasureDto dto, Guid tenantId, string updatedBy)
    {
        var entity = UnitOfMeasureMapper.ToEntity(dto);
        entity.TenantId = tenantId;
        entity.UpdatedBy = updatedBy;
        
        var updated = await _repository.UpdateAsync(entity);
        return UnitOfMeasureMapper.ToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        return await _repository.DeleteAsync(id, tenantId);
    }
}