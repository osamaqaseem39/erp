using ERP.Domain.Sales;
using ERP.Shared.DTOs.Sales;

namespace ERP.Api.Mappers.Sales;

public class POSSessionMapper : BaseMapper<POSSession, POSSessionDto>
{
    public override POSSessionDto Map(POSSession source)
    {
        if (source == null) return null!;

        return new POSSessionDto
        {
            Id = source.Id,
            SessionNumber = source.SessionNumber,
            CashierId = source.CashierId,
            TerminalId = source.TerminalId,
            StartTime = source.StartTime,
            EndTime = source.EndTime,
            OpeningBalance = source.OpeningBalance,
            ClosingBalance = source.ClosingBalance,
            ExpectedBalance = source.ExpectedBalance,
            Difference = source.Difference,
            Status = source.Status,
            Notes = source.Notes,
            TenantId = source.TenantId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public override POSSession Map(POSSessionDto destination)
    {
        if (destination == null) return null!;

        return new POSSession
        {
            Id = destination.Id.HasValue ? destination.Id.Value : Guid.NewGuid(),
            SessionNumber = destination.SessionNumber,
            CashierId = destination.CashierId,
            TerminalId = destination.TerminalId,
            StartTime = destination.StartTime,
            EndTime = destination.EndTime,
            OpeningBalance = destination.OpeningBalance,
            ClosingBalance = destination.ClosingBalance,
            ExpectedBalance = destination.ExpectedBalance,
            Difference = destination.Difference,
            Status = destination.Status,
            Notes = destination.Notes,
            TenantId = destination.TenantId,
            CreatedAt = destination.CreatedAt,
            UpdatedAt = destination.UpdatedAt
        };
    }
}

