using ERP.Domain.SCM;
using ERP.Shared.DTOs.Inventory;    

namespace ERP.Shared.Mappers.Inventory;

public class BarcodeMapper : IMapper<Barcode, BarcodeDto>
{
    public BarcodeDto ToDto(Barcode entity)
    {
        return new BarcodeDto
        {
            Id = entity.Id,
            ItemId = entity.ItemId ?? Guid.Empty,
            VariantId = entity.VariantId,
            Barcode = entity.BarcodeValue,
            BarcodeType = entity.BarcodeType,
            IsPrimary = entity.IsPrimary,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public Barcode ToEntity(BarcodeDto dto)
    {
        return new Barcode
        {
            Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            ItemId = dto.ItemId,
            VariantId = dto.VariantId,
            BarcodeValue = dto.Barcode,
            BarcodeType = dto.BarcodeType,
            IsPrimary = dto.IsPrimary,
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            CreatedBy = dto.CreatedBy,
            UpdatedBy = dto.UpdatedBy
        };
    }
}