using ERP.Application.SCM.Interfaces;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Shared.DTOs.Inventory;
using ERP.Shared.Mappers;
using ERP.Shared.Mappers.Inventory;
using ERP.Domain.SCM;

namespace ERP.Application.SCM.Services;

public class BarcodeService : IBarcodeService
{
    private readonly IBarcodeRepository _repository;
    private readonly BarcodeMapper _mapper;

    public BarcodeService(IBarcodeRepository repository, BarcodeMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BarcodeDto>> GetByItemAsync(Guid? itemId, Guid? variantId, Guid tenantId)
    {
        var entities = await _repository.GetByItemAsync(itemId, variantId, tenantId);
        return entities.Select(_mapper.ToDto);
    }

    public async Task<BarcodeDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity != null ? _mapper.ToDto(entity) : null;
    }

    public async Task<BarcodeDto> CreateAsync(BarcodeDto dto, Guid tenantId, string createdBy)
    {
        var entity = _mapper.ToEntity(dto);
        entity.TenantId = tenantId;
        entity.CreatedBy = createdBy;
        
        var created = await _repository.CreateAsync(entity);
        return _mapper.ToDto(created);
    }

    public async Task<BarcodeDto> UpdateAsync(BarcodeDto dto, Guid tenantId, string updatedBy)
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