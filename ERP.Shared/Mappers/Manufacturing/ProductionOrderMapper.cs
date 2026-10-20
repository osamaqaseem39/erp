using ERP.Domain.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;
using ERP.Shared.Mappers;

namespace ERP.Shared.Mappers.Manufacturing;

public static class ProductionOrderMapper
{
    public static ProductionOrderDto ToDto(ProductionOrder entity)
    {
        return new ProductionOrderDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            ProductionOrderNumber = entity.ProductionOrderNumber,
            OrderNumber = entity.OrderNumber,
            ItemId = entity.ItemId,
            WorkCenterId = entity.WorkCenterId,
            Quantity = entity.Quantity,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            PlannedStartDate = entity.PlannedStartDate,
            PlannedEndDate = entity.PlannedEndDate,
            ActualStartDate = entity.ActualStartDate,
            ActualEndDate = entity.ActualEndDate,
            Status = entity.Status,
            Priority = entity.Priority,
            CompletedQuantity = entity.CompletedQuantity,
            Notes = entity.Notes,
            CancellationReason = entity.CancellationReason,
            ItemName = entity.Item?.ItemName,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static ProductionOrder ToEntity(CreateProductionOrderDto dto, Guid tenantId, string createdBy)
    {
        return new ProductionOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CompanyId = dto.CompanyId,
            ProductionOrderNumber = dto.ProductionOrderNumber ?? string.Empty,
            OrderNumber = dto.OrderNumber ?? string.Empty,
            ItemId = dto.ItemId,
            WorkCenterId = dto.WorkCenterId,
            Quantity = dto.Quantity ?? 0m,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            PlannedStartDate = dto.PlannedStartDate,
            PlannedEndDate = dto.PlannedEndDate,
            Status = dto.Status ?? "Planned",
            Priority = dto.Priority ?? "Medium",
            Notes = dto.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static void UpdateEntity(UpdateProductionOrderDto dto, ProductionOrder entity, string updatedBy)
    {
        if (!string.IsNullOrEmpty(dto.ProductionOrderNumber))
            entity.ProductionOrderNumber = dto.ProductionOrderNumber;
        
        if (!string.IsNullOrEmpty(dto.OrderNumber))
            entity.OrderNumber = dto.OrderNumber;
        
        if (dto.ItemId.HasValue)
            entity.ItemId = dto.ItemId;
        
        if (dto.WorkCenterId.HasValue)
            entity.WorkCenterId = dto.WorkCenterId;
        
        if (dto.Quantity.HasValue)
            entity.Quantity = dto.Quantity.Value;
        
        if (dto.StartDate.HasValue)
            entity.StartDate = dto.StartDate;
        
        if (dto.EndDate.HasValue)
            entity.EndDate = dto.EndDate;
        
        if (dto.PlannedStartDate.HasValue)
            entity.PlannedStartDate = dto.PlannedStartDate;
        
        if (dto.PlannedEndDate.HasValue)
            entity.PlannedEndDate = dto.PlannedEndDate;
        
        // Status only via start/complete/cancel workflow
        
        if (!string.IsNullOrEmpty(dto.Priority))
            entity.Priority = dto.Priority;
        
        entity.Notes = dto.Notes;
        
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;
    }
}