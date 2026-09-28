using ERP.Domain.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;

namespace ERP.Api.Mappers.Manufacturing;

public class ProductionOrderMapper : BaseMapper<ProductionOrder, ProductionOrderDto>
{
    public override ProductionOrderDto Map(ProductionOrder source)
    {
        if (source == null) return null!;

        return new ProductionOrderDto
        {
            Id = source.Id,
            ProductionOrderNumber = source.ProductionOrderNumber,
            OrderNumber = source.OrderNumber,
            ItemId = source.ItemId,
            WorkCenterId = source.WorkCenterId,
            Quantity = source.Quantity,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            PlannedStartDate = source.PlannedStartDate,
            PlannedEndDate = source.PlannedEndDate,
            ActualStartDate = source.ActualStartDate,
            ActualEndDate = source.ActualEndDate,
            Status = source.Status,
            Priority = source.Priority,
            CompletedQuantity = source.CompletedQuantity,
            Notes = source.Notes,
            CancellationReason = source.CancellationReason,
            TenantId = source.TenantId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public override ProductionOrder Map(ProductionOrderDto destination)
    {
        if (destination == null) return null!;

        return new ProductionOrder
        {
            Id = destination.Id != null && destination.Id != Guid.Empty ? destination.Id.Value : Guid.NewGuid(),
            ProductionOrderNumber = destination.ProductionOrderNumber,
            OrderNumber = destination.OrderNumber,
            ItemId = destination.ItemId,
            WorkCenterId = destination.WorkCenterId,
            Quantity = destination.Quantity,
            StartDate = destination.StartDate,
            EndDate = destination.EndDate,
            PlannedStartDate = destination.PlannedStartDate,
            PlannedEndDate = destination.PlannedEndDate,
            ActualStartDate = destination.ActualStartDate,
            ActualEndDate = destination.ActualEndDate,
            Status = destination.Status,
            Priority = destination.Priority,
            CompletedQuantity = destination.CompletedQuantity,
            Notes = destination.Notes,
            CancellationReason = destination.CancellationReason,
            TenantId = destination.TenantId != Guid.Empty ? destination.TenantId : Guid.Empty,
            CreatedAt = destination.CreatedAt,
            UpdatedAt = destination.UpdatedAt
        };
    }
}

