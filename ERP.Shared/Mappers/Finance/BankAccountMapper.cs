using ERP.Domain.Finance;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.Mappers;

namespace ERP.Shared.Mappers.Finance;

public static class BankAccountMapper
{
    public static BankAccountDto ToDto(BankAccount entity)
    {
        return new BankAccountDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CompanyId = entity.CompanyId,
            BankId = entity.BankId,
            AccountName = entity.AccountName,
            AccountNumber = entity.AccountNumber,
            AccountType = entity.AccountType,
            Currency = entity.Currency,
            Balance = entity.Balance,
            IBAN = entity.IBAN,
            SWIFT = entity.SWIFT,
            GLAccountId = entity.GLAccountId,
            IsActive = entity.IsActive,
            BankName = entity.Bank?.BankName ?? string.Empty,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static BankAccount ToEntity(CreateBankAccountDto dto, Guid tenantId, string createdBy)
    {
        return new BankAccount
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CompanyId = dto.CompanyId,
            BankId = dto.BankId,
            AccountName = dto.AccountName,
            AccountNumber = dto.AccountNumber,
            AccountType = dto.AccountType,
            Currency = dto.Currency,
            Balance = dto.Balance,
            IBAN = dto.IBAN,
            SWIFT = dto.SWIFT,
            GLAccountId = dto.GLAccountId,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static void UpdateEntity(UpdateBankAccountDto dto, BankAccount entity, string updatedBy)
    {
        entity.BankId = dto.BankId;
        entity.AccountName = dto.AccountName;
        entity.AccountNumber = dto.AccountNumber;
        entity.AccountType = dto.AccountType;
        entity.Currency = dto.Currency;
        entity.Balance = dto.Balance;
        entity.IBAN = dto.IBAN;
        entity.SWIFT = dto.SWIFT;
        entity.GLAccountId = dto.GLAccountId;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;
    }
}