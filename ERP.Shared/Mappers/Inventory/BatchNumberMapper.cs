using ERP.Domain.SCM;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Shared.Mappers.Inventory;

public class BatchNumberMapper : IMapper<BatchNumber, BatchNumberDto>
{
    public BatchNumberDto ToDto(BatchNumber entity)
    {
        return new BatchNumberDto
        {
            Id = entity.Id,
            ItemId = entity.ItemId,
            BatchNumber = entity.BatchNumberValue,
            ManufacturingDate = entity.ManufacturingDate ?? DateTime.MinValue,
            ExpiryDate = entity.ExpiryDate ?? DateTime.MinValue,
            Quantity = entity.Quantity,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            ItemName = entity.Item?.ItemName
        };
    }

    public BatchNumber ToEntity(BatchNumberDto dto)
    {
        return new BatchNumber
        {
            Id = dto.Id,
            ItemId = dto.ItemId,
            BatchNumberValue = dto.BatchNumber,
            ManufacturingDate = dto.ManufacturingDate,
            ExpiryDate = dto.ExpiryDate,
            Quantity = dto.Quantity,
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            CreatedBy = dto.CreatedBy,
            UpdatedBy = dto.UpdatedBy
        };
    }
}