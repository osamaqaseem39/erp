using ERP.Domain.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;
using ERP.Shared.Mappers;

namespace ERP.Shared.Mappers.Manufacturing;

public static class WorkCenterMapper
{
    public static WorkCenterDto ToDto(WorkCenter entity)
    {
        return new WorkCenterDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            WorkCenterCode = entity.WorkCenterCode,
            WorkCenterName = entity.WorkCenterName,
            Description = entity.Description,
            WorkCenterType = entity.WorkCenterType,
            Capacity = entity.Capacity,
            CapacityUnit = entity.CapacityUnit,
            IsActive = entity.IsActive,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static WorkCenter ToEntity(CreateWorkCenterDto dto, Guid tenantId, string createdBy)
    {
        return new WorkCenter
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CompanyId = dto.CompanyId,
            WorkCenterCode = dto.WorkCenterCode,
            WorkCenterName = dto.WorkCenterName,
            Description = dto.Description,
            WorkCenterType = dto.WorkCenterType,
            Capacity = dto.Capacity ?? 0m,
            CapacityUnit = dto.CapacityUnit,
            IsActive = true,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static void UpdateEntity(UpdateWorkCenterDto dto, WorkCenter entity, string updatedBy)
    {
        entity.WorkCenterCode = dto.WorkCenterCode;
        entity.WorkCenterName = dto.WorkCenterName;
        entity.Description = dto.Description;
        entity.WorkCenterType = dto.WorkCenterType;
            entity.Capacity = dto.Capacity ?? 0m;
        entity.CapacityUnit = dto.CapacityUnit ?? string.Empty;
        entity.IsActive = dto.IsActive ?? true;
        entity.Notes = dto.Notes;
        
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;
    }
}