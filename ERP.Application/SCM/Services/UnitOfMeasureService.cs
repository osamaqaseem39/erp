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
        if (tenantId == Guid.Empty)
            return Array.Empty<UnitOfMeasureDto>();

        var entities = await _repository.GetAllAsync(tenantId, isActive);
        return entities.Select(UnitOfMeasureMapper.ToDto);
    }

    public async Task<UnitOfMeasureDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return null;

        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity != null ? UnitOfMeasureMapper.ToDto(entity) : null;
    }

    public async Task<UnitOfMeasureDto> CreateAsync(UnitOfMeasureDto dto, Guid tenantId, string createdBy)
    {
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Tenant is required.");

        var entity = UnitOfMeasureMapper.ToEntity(dto);
        entity.TenantId = tenantId;
        entity.CreatedBy = createdBy;

        var created = await _repository.CreateAsync(entity);
        return UnitOfMeasureMapper.ToDto(created);
    }

    public async Task<UnitOfMeasureDto> UpdateAsync(UnitOfMeasureDto dto, Guid tenantId, string updatedBy)
    {
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Tenant is required.");

        var existing = await _repository.GetByIdAsync(dto.Id, tenantId)
            ?? throw new KeyNotFoundException($"Unit of measure '{dto.Id}' not found.");

        existing.UnitCode = dto.Code;
        existing.UnitName = dto.Name;
        existing.Description = dto.Description;
        existing.ConversionFactor = dto.ConversionFactor;
        existing.BaseUnitId = dto.BaseUnitId;
        existing.IsActive = dto.IsActive;
        existing.UpdatedBy = updatedBy;

        var updated = await _repository.UpdateAsync(existing);
        return UnitOfMeasureMapper.ToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return false;

        return await _repository.DeleteAsync(id, tenantId);
    }
}
