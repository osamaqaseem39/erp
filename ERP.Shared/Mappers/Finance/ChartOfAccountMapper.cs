using ERP.Domain.Finance;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.Mappers;

namespace ERP.Shared.Mappers.Finance;

public static class ChartOfAccountMapper
{
    public static ChartOfAccountDto ToDto(ChartOfAccount entity)
    {
        return new ChartOfAccountDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            AccountId = entity.AccountId,
            AccountCode = entity.AccountCode,
            AccountName = entity.AccountName,
            AccountType = entity.AccountType,
            ParentAccountId = entity.ParentAccountId,
            ParentAccountName = entity.ParentAccount?.AccountName,
            CurrencyCode = entity.CurrencyCode,
            IsActive = entity.IsActive,
            IsPostingAllowed = entity.IsPostingAllowed,
            CreatedAt = entity.CreatedAt,
            CreatedBy = !string.IsNullOrEmpty(entity.CreatedBy) ? Guid.Parse(entity.CreatedBy) : null,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = !string.IsNullOrEmpty(entity.UpdatedBy) ? Guid.Parse(entity.UpdatedBy) : null
        };
    }

    public static ChartOfAccount ToEntity(CreateChartOfAccountDto dto, Guid tenantId, string createdBy)
    {
        return new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AccountId = Guid.NewGuid(),
            AccountCode = dto.AccountCode,
            AccountName = dto.AccountName,
            AccountType = dto.AccountType,
            ParentAccountId = dto.ParentAccountId ?? Guid.Empty,
            CurrencyCode = dto.CurrencyCode,
            IsActive = dto.IsActive,
            IsPostingAllowed = dto.IsPostingAllowed,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static void UpdateEntity(UpdateChartOfAccountDto dto, ChartOfAccount entity, string updatedBy)
    {
        if (!string.IsNullOrEmpty(dto.AccountCode))
            entity.AccountCode = dto.AccountCode;
        
        if (!string.IsNullOrEmpty(dto.AccountName))
            entity.AccountName = dto.AccountName;
        
        if (!string.IsNullOrEmpty(dto.AccountType))
            entity.AccountType = dto.AccountType;
        
        if (dto.ParentAccountId.HasValue)
            entity.ParentAccountId = dto.ParentAccountId.Value;
        
        if (!string.IsNullOrEmpty(dto.CurrencyCode))
            entity.CurrencyCode = dto.CurrencyCode;
        
        if (dto.IsActive.HasValue)
            entity.IsActive = dto.IsActive.Value;
        
        if (dto.IsPostingAllowed.HasValue)
            entity.IsPostingAllowed = dto.IsPostingAllowed.Value;
        
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;
    }
}