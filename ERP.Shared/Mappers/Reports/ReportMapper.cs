using ERP.Domain.Reports;
using ERP.Shared.DTOs.Reports;
using ERP.Shared.Mappers;

namespace ERP.Shared.Mappers.Reports;

public static class ReportMapper
{
    public static ReportDto ToDto(Report entity)
    {
        return new ReportDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            ReportName = entity.ReportName,
            ReportCode = entity.ReportCode,
            Description = entity.Description,
            ReportType = entity.ReportType,
            Query = entity.Query,
            Parameters = entity.Parameters,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static Report ToEntity(CreateReportDto dto, Guid tenantId, string createdBy)
    {
        return new Report
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReportName = dto.ReportName,
            ReportCode = dto.ReportCode,
            Description = dto.Description,
            ReportType = dto.ReportType,
            Query = dto.Query,
            Parameters = dto.Parameters,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static void UpdateEntity(UpdateReportDto dto, Report entity, string updatedBy)
    {
        entity.ReportName = dto.ReportName;
        entity.ReportCode = dto.ReportCode;
        entity.Description = dto.Description;
        entity.ReportType = dto.ReportType;
        entity.Query = dto.Query;
        entity.Parameters = dto.Parameters;
        entity.IsActive = dto.IsActive;
        
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;
    }
}