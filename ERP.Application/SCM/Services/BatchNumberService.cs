using ERP.Application.SCM.Interfaces;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Shared.DTOs.Inventory;
using ERP.Shared.Mappers.Inventory;

namespace ERP.Application.SCM.Services;

public class BatchNumberService : IBatchNumberService
{
    private readonly IBatchNumberRepository _repository;
    private readonly BatchNumberMapper _mapper;

    public BatchNumberService(IBatchNumberRepository repository, BatchNumberMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BatchNumberDto>> GetByItemAsync(Guid itemId, Guid tenantId)
    {
        var entities = await _repository.GetByItemAsync(itemId, tenantId);
        return entities.Select(_mapper.ToDto);
    }

    public async Task<BatchNumberDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity != null ? _mapper.ToDto(entity) : null;
    }

    public async Task<BatchNumberDto> CreateAsync(BatchNumberDto dto, Guid tenantId, string createdBy)
    {
        var entity = _mapper.ToEntity(dto);
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        entity.TenantId = tenantId;
        entity.CreatedBy = createdBy;

        var created = await _repository.CreateAsync(entity);
        return _mapper.ToDto(created);
    }

    public async Task<BatchNumberDto> UpdateAsync(BatchNumberDto dto, Guid tenantId, string updatedBy)
    {
        var existing = await _repository.GetByIdAsync(dto.Id, tenantId)
            ?? throw new KeyNotFoundException($"Batch number '{dto.Id}' not found.");

        existing.ItemId = dto.ItemId;
        existing.BatchNumberValue = dto.BatchNumber;
        existing.ManufacturingDate = dto.ManufacturingDate;
        existing.ExpiryDate = dto.ExpiryDate;
        existing.Quantity = dto.Quantity;
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
