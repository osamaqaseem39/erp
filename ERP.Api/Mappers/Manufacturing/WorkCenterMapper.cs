using ERP.Domain.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;

namespace ERP.Api.Mappers.Manufacturing;

public class WorkCenterMapper : BaseMapper<WorkCenter, WorkCenterDto>
{
    public override WorkCenterDto Map(WorkCenter source)
    {
        if (source == null) return null!;

        return new WorkCenterDto
        {
            Id = source.Id,
            WorkCenterCode = source.WorkCenterCode,
            Code = source.WorkCenterCode,
            WorkCenterName = source.WorkCenterName,
            Name = source.WorkCenterName,
            Description = source.Description,
            WorkCenterType = source.WorkCenterType,
            Capacity = source.Capacity,
            CapacityUnit = source.CapacityUnit,
            Notes = source.Notes,
            IsActive = source.IsActive,
            TenantId = source.TenantId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public override WorkCenter Map(WorkCenterDto destination)
    {
        if (destination == null) return null!;

        return new WorkCenter
        {
            Id = destination.Id != null && destination.Id != Guid.Empty ? destination.Id.Value : Guid.NewGuid(),
            WorkCenterCode = destination.WorkCenterCode,
            WorkCenterName = destination.WorkCenterName,
            Description = destination.Description,
            WorkCenterType = destination.WorkCenterType,
            Capacity = destination.Capacity,
            CapacityUnit = destination.CapacityUnit,
            Notes = destination.Notes,
            IsActive = destination.IsActive,
            TenantId = destination.TenantId != Guid.Empty ? destination.TenantId : Guid.Empty,
            CreatedAt = destination.CreatedAt,
            UpdatedAt = destination.UpdatedAt
        };
    }
}

