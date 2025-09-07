using ERP.Domain.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;
using ERP.Shared.Mappers;

namespace ERP.Shared.Mappers.Manufacturing;

public static class ProductionRunMapper
{
    public static ProductionRunDto ToDto(ProductionRun entity)
    {
        return new ProductionRunDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            ProductionOrderId = entity.ProductionOrderId,
            RunNumber = entity.RunNumber,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            QuantityProduced = entity.QuantityProduced,
            QuantityRejected = entity.QuantityRejected,
            Status = entity.Status,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static ProductionRun ToEntity(CreateProductionRunDto dto, Guid tenantId, string createdBy)
    {
        return new ProductionRun
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProductionOrderId = dto.ProductionOrderId,
            RunNumber = dto.RunNumber ?? string.Empty,
            StartTime = dto.StartTime ?? DateTime.UtcNow,
            EndTime = dto.EndTime,
            QuantityProduced = dto.QuantityProduced ?? 0m,
            QuantityRejected = dto.QuantityRejected ?? 0m,
            Status = dto.Status ?? "InProgress",
            Notes = dto.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static void UpdateEntity(UpdateProductionRunDto dto, ProductionRun entity, string updatedBy)
    {
        if (!string.IsNullOrEmpty(dto.RunNumber))
            entity.RunNumber = dto.RunNumber;
        
        entity.StartTime = dto.StartTime;
        entity.EndTime = dto.EndTime;
        entity.QuantityProduced = dto.QuantityProduced;
        entity.QuantityRejected = dto.QuantityRejected;
        
        if (!string.IsNullOrEmpty(dto.Status))
            entity.Status = dto.Status;
        
        entity.Notes = dto.Notes;
        
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;
    }
}