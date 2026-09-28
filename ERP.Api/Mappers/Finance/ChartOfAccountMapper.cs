using ERP.Domain.Finance;
using ERP.Shared.DTOs.Finance;

namespace ERP.Api.Mappers.Finance;

public class ChartOfAccountMapper : BaseMapper<ChartOfAccount, ChartOfAccountDto>
{
    public override ChartOfAccountDto Map(ChartOfAccount source)
    {
        if (source == null) return null!;

        return new ChartOfAccountDto
        {
            Id = source.Id,
            AccountId = source.AccountId,
            TenantId = source.TenantId,
            AccountCode = source.AccountCode,
            AccountName = source.AccountName,
            AccountType = source.AccountType,
            ParentAccountId = source.ParentAccountId,
            CurrencyCode = source.CurrencyCode,
            IsActive = source.IsActive,
            IsPostingAllowed = source.IsPostingAllowed,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt,
            CreatedBy = !string.IsNullOrEmpty(source.CreatedBy) && Guid.TryParse(source.CreatedBy, out var createdByGuid) ? createdByGuid : null,
            UpdatedBy = !string.IsNullOrEmpty(source.UpdatedBy) && Guid.TryParse(source.UpdatedBy, out var updatedByGuid) ? updatedByGuid : null
        };
    }

    public override ChartOfAccount Map(ChartOfAccountDto destination)
    {
        if (destination == null) return null!;

        return new ChartOfAccount
        {
            Id = destination.Id,
            AccountId = destination.AccountId ?? Guid.NewGuid(),
            TenantId = destination.TenantId.HasValue && destination.TenantId.Value != Guid.Empty ? destination.TenantId.Value : Guid.Empty,
            AccountCode = destination.AccountCode ?? string.Empty,
            AccountName = destination.AccountName ?? string.Empty,
            AccountType = destination.AccountType ?? string.Empty,
            ParentAccountId = destination.ParentAccountId,
            CurrencyCode = destination.CurrencyCode,
            IsActive = destination.IsActive ?? true,
            IsPostingAllowed = destination.IsPostingAllowed ?? true,
            CreatedAt = destination.CreatedAt ?? DateTime.UtcNow,
            UpdatedAt = destination.UpdatedAt,
            CreatedBy = destination.CreatedBy?.ToString(),
            UpdatedBy = destination.UpdatedBy?.ToString()
        };
    }
}

