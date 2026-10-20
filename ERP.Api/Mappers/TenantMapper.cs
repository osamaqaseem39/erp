using ERP.Domain.Core;
using ERP.Shared.DTOs.Core;

namespace ERP.Api.Mappers;

public class TenantMapper : BaseMapper<Tenant, TenantDto>
{
    public override TenantDto Map(Tenant source)
    {
        if (source == null) return null!;

        return new TenantDto
        {
            Id = source.Id,
            TenantId = source.TenantId,
            TenantName = source.TenantName,
            Domain = source.Domain,
            Industry = source.Industry,
            Country = source.Country,
            Status = source.Status,
            DatabaseName = source.DatabaseName,
            IsActive = source.IsActive,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt,
            CreatedBy = source.CreatedBy,
            UpdatedBy = source.UpdatedBy
        };
    }

    public override Tenant Map(TenantDto destination)
    {
        if (destination == null) return null!;

        return new Tenant
        {
            Id = destination.Id,
            TenantId = destination.TenantId,
            TenantName = destination.TenantName,
            Domain = destination.Domain,
            Industry = destination.Industry,
            Country = destination.Country,
            Status = destination.Status,
            DatabaseName = destination.DatabaseName,
            ConnectionString = string.Empty,
            IsActive = destination.IsActive,
            CreatedAt = destination.CreatedAt,
            UpdatedAt = destination.UpdatedAt,
            CreatedBy = destination.CreatedBy,
            UpdatedBy = destination.UpdatedBy
        };
    }
}