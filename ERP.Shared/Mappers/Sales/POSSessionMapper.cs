using ERP.Domain.Sales;
using ERP.Shared.DTOs.Sales;
using ERP.Shared.Mappers;

namespace ERP.Shared.Mappers.Sales;

public static class POSSessionMapper
{
    public static POSSessionDto ToDto(POSSession entity)
    {
        return new POSSessionDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            SessionNumber = entity.SessionNumber,
            CashierId = entity.CashierId,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            OpeningBalance = entity.OpeningBalance,
            ClosingBalance = entity.ClosingBalance,
            Status = entity.Status,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static POSSession ToEntity(CreatePOSSessionDto dto, Guid tenantId, string createdBy)
    {
        return new POSSession
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SessionNumber = dto.SessionNumber,
            CashierId = dto.CashierId,
            StartTime = dto.StartTime,
            OpeningBalance = dto.OpeningBalance,
            Status = dto.Status ?? "Open",
            Notes = dto.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static void UpdateEntity(UpdatePOSSessionDto dto, POSSession entity, string updatedBy)
    {
        entity.SessionNumber = dto.SessionNumber;
        entity.CashierId = dto.CashierId;
        if (dto.StartTime.HasValue) entity.StartTime = dto.StartTime.Value;
        entity.EndTime = dto.EndTime;
        if (dto.OpeningBalance.HasValue) entity.OpeningBalance = dto.OpeningBalance.Value;
        entity.ClosingBalance = dto.ClosingBalance;
        // Status only via Open/Close DocumentWorkflow
        entity.Notes = dto.Notes;
        
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;
    }
}