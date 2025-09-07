using ERP.Domain.SCM;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Shared.Mappers.Inventory;

public class SerialNumberMapper : IMapper<SerialNumber, SerialNumberDto>
{
    public SerialNumberDto ToDto(SerialNumber entity)
    {
        return new SerialNumberDto
        {
            Id = entity.Id,
            ItemId = entity.ItemId,
            SerialNumber = entity.SerialNumberValue,
            Status = entity.Status,
            Location = entity.Location,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            ItemName = entity.Item?.ItemName
        };
    }

    public SerialNumber ToEntity(SerialNumberDto dto)
    {
        return new SerialNumber
        {
            Id = dto.Id,
            ItemId = dto.ItemId,
            SerialNumberValue = dto.SerialNumber,
            Status = dto.Status,
            Location = dto.Location,
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            CreatedBy = dto.CreatedBy,
            UpdatedBy = dto.UpdatedBy
        };
    }
}