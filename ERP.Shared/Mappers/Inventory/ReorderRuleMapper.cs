using ERP.Domain.SCM;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Shared.Mappers.Inventory;

public class ReorderRuleMapper : IMapper<ReorderRule, ReorderRuleDto>
{
    public ReorderRuleDto ToDto(ReorderRule entity)
    {
        return new ReorderRuleDto
        {
            Id = entity.Id,
            ItemId = entity.ItemId,
            WarehouseId = entity.WarehouseId,
            MinStockLevel = entity.MinStockLevel,
            MaxStockLevel = entity.MaxStockLevel,
            ReorderPoint = entity.ReorderPoint,
            ReorderQuantity = entity.ReorderQuantity,
            LeadTimeDays = entity.LeadTimeDays,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            ItemName = entity.Item?.ItemName,
            WarehouseName = entity.Warehouse?.WarehouseName
        };
    }

    public ReorderRule ToEntity(ReorderRuleDto dto)
    {
        return new ReorderRule
        {
            Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            ItemId = dto.ItemId,
            WarehouseId = dto.WarehouseId,
            MinStockLevel = dto.MinStockLevel,
            MaxStockLevel = dto.MaxStockLevel,
            ReorderPoint = dto.ReorderPoint,
            ReorderQuantity = dto.ReorderQuantity,
            LeadTimeDays = dto.LeadTimeDays,
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            CreatedBy = dto.CreatedBy,
            UpdatedBy = dto.UpdatedBy
        };
    }
}