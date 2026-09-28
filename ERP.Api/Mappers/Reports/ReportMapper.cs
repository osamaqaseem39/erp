using ERP.Domain.Reports;
using ERP.Shared.DTOs.Reports;

namespace ERP.Api.Mappers.Reports;

public class ReportMapper : BaseMapper<Report, ReportDto>
{
    public override ReportDto Map(Report source)
    {
        if (source == null) return null!;

        return new ReportDto
        {
            Id = source.Id,
            ReportName = source.ReportName,
            Name = source.Name,
            ReportCode = source.ReportCode,
            Description = source.Description,
            ReportType = source.ReportType,
            Category = source.Category,
            Query = source.Query,
            Parameters = source.Parameters,
            OutputFormat = source.OutputFormat,
            IsActive = source.IsActive,
            IsPublic = source.IsPublic,
            Notes = source.Notes,
            TenantId = source.TenantId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public override Report Map(ReportDto destination)
    {
        if (destination == null) return null!;

        return new Report
        {
            Id = destination.Id.HasValue && destination.Id.Value != Guid.Empty ? destination.Id.Value : Guid.NewGuid(),
            ReportName = destination.ReportName,
            Name = destination.Name,
            ReportCode = destination.ReportCode,
            Description = destination.Description,
            ReportType = destination.ReportType,
            Category = destination.Category,
            Query = destination.Query,
            Parameters = destination.Parameters,
            OutputFormat = destination.OutputFormat,
            IsActive = destination.IsActive,
            IsPublic = destination.IsPublic,
            Notes = destination.Notes,
            TenantId = destination.TenantId != Guid.Empty ? destination.TenantId : Guid.Empty,
            CreatedAt = destination.CreatedAt,
            UpdatedAt = destination.UpdatedAt
        };
    }
}

