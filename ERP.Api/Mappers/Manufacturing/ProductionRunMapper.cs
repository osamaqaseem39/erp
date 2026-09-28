using ERP.Domain.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;

namespace ERP.Api.Mappers.Manufacturing;

public class ProductionRunMapper : BaseMapper<ProductionRun, ProductionRunDto>
{
    public override ProductionRunDto Map(ProductionRun source)
    {
        if (source == null) return null!;

        return new ProductionRunDto
        {
            Id = source.Id,
            ProductionOrderId = source.ProductionOrderId,
            RunNumber = source.RunNumber,
            StartTime = source.StartTime,
            EndTime = source.EndTime,
            QuantityProduced = source.QuantityProduced,
            QuantityRejected = source.QuantityRejected,
            Status = source.Status,
            Notes = source.Notes,
            TenantId = source.TenantId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public override ProductionRun Map(ProductionRunDto destination)
    {
        if (destination == null) return null!;

        return new ProductionRun
        {
            Id = destination.Id != null && destination.Id != Guid.Empty ? destination.Id.Value : Guid.NewGuid(),
            ProductionOrderId = destination.ProductionOrderId,
            RunNumber = destination.RunNumber,
            StartTime = destination.StartTime,
            EndTime = destination.EndTime,
            QuantityProduced = destination.QuantityProduced,
            QuantityRejected = destination.QuantityRejected,
            Status = destination.Status,
            Notes = destination.Notes,
            TenantId = destination.TenantId != Guid.Empty ? destination.TenantId : Guid.Empty,
            CreatedAt = destination.CreatedAt,
            UpdatedAt = destination.UpdatedAt
        };
    }
}

