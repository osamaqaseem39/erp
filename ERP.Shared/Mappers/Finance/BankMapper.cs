using ERP.Domain.Finance;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.Mappers;

namespace ERP.Shared.Mappers.Finance;

public static class BankMapper
{
    public static BankDto ToDto(Bank entity)
    {
        return new BankDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CompanyId = entity.CompanyId,
            BankName = entity.BankName,
            BankCode = entity.BankCode,
            SwiftCode = entity.SwiftCode ?? string.Empty,
            Address = entity.Address ?? string.Empty,
            City = entity.City ?? string.Empty,
            State = entity.State ?? string.Empty,
            Country = entity.Country ?? string.Empty,
            PostalCode = entity.PostalCode ?? string.Empty,
            Phone = entity.Phone ?? string.Empty,
            Email = entity.Email ?? string.Empty,
            Website = entity.Website ?? string.Empty,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static Bank ToEntity(CreateBankDto dto, Guid tenantId, string createdBy)
    {
        return new Bank
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CompanyId = dto.CompanyId,
            BankName = dto.BankName,
            BankCode = dto.BankCode,
            SwiftCode = dto.SwiftCode,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            Country = dto.Country,
            PostalCode = dto.PostalCode,
            Phone = dto.Phone,
            Email = dto.Email,
            Website = dto.Website,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static void UpdateEntity(UpdateBankDto dto, Bank entity, string updatedBy)
    {
        entity.BankName = dto.BankName;
        entity.BankCode = dto.BankCode;
        entity.SwiftCode = dto.SwiftCode;
        entity.Address = dto.Address;
        entity.City = dto.City;
        entity.State = dto.State;
        entity.Country = dto.Country;
        entity.PostalCode = dto.PostalCode;
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        entity.Website = dto.Website;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;
    }
}