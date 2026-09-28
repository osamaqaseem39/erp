using ERP.Domain.Finance;
using ERP.Shared.DTOs.Finance;

namespace ERP.Api.Mappers.Finance;

public class JournalMapper : BaseMapper<Journal, JournalDto>
{
    public override JournalDto Map(Journal source)
    {
        if (source == null) return null!;

        return new JournalDto
        {
            Id = source.Id,
            JournalId = source.JournalId,
            TenantId = source.TenantId,
            JournalType = source.JournalType,
            ReferenceNo = source.ReferenceNo,
            JournalDate = source.JournalDate,
            Description = source.Description,
            CurrencyCode = source.CurrencyCode,
            ExchangeRate = source.ExchangeRate,
            TotalDebit = source.TotalDebit,
            TotalCredit = source.TotalCredit,
            Status = source.Status,
            CreatedAt = source.CreatedAt,
            CreatedBy = !string.IsNullOrEmpty(source.CreatedBy) && Guid.TryParse(source.CreatedBy, out var createdByGuid) ? createdByGuid : null,
            PostedAt = source.PostedAt,
            PostedBy = source.PostedBy,
            IsLocked = source.IsLocked
        };
    }

    public override Journal Map(JournalDto destination)
    {
        if (destination == null) return null!;

        return new Journal
        {
            Id = destination.Id,
            JournalId = destination.JournalId ?? Guid.NewGuid(),
            TenantId = destination.TenantId.HasValue && destination.TenantId.Value != Guid.Empty ? destination.TenantId.Value : Guid.Empty,
            JournalType = destination.JournalType ?? string.Empty,
            ReferenceNo = destination.ReferenceNo,
            JournalDate = destination.JournalDate ?? DateTime.UtcNow,
            Description = destination.Description,
            CurrencyCode = destination.CurrencyCode,
            ExchangeRate = destination.ExchangeRate,
            TotalDebit = destination.TotalDebit ?? 0,
            TotalCredit = destination.TotalCredit ?? 0,
            Status = destination.Status ?? "Draft",
            CreatedAt = destination.CreatedAt ?? DateTime.UtcNow,
            CreatedBy = destination.CreatedBy?.ToString(),
            PostedAt = destination.PostedAt,
            PostedBy = destination.PostedBy,
            IsLocked = destination.IsLocked ?? false
        };
    }
}

