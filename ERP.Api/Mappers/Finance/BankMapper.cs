using ERP.Domain.Finance;
using ERP.Shared.DTOs.Finance;

namespace ERP.Api.Mappers.Finance;

public class BankMapper : BaseMapper<Bank, BankDto>
{
    public override BankDto Map(Bank source)
    {
        if (source == null) return null!;

        return new BankDto
        {
            Id = source.Id,
            CompanyId = source.CompanyId,
            BankName = source.BankName,
            BankCode = source.BankCode,
            SwiftCode = source.SwiftCode ?? string.Empty,
            Address = source.Address ?? string.Empty,
            City = source.City ?? string.Empty,
            State = source.State ?? string.Empty,
            Country = source.Country ?? string.Empty,
            PostalCode = source.PostalCode ?? string.Empty,
            Phone = source.Phone ?? string.Empty,
            Email = source.Email ?? string.Empty,
            Website = source.Website ?? string.Empty,
            IsActive = source.IsActive,
            TenantId = source.TenantId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public override Bank Map(BankDto destination)
    {
        if (destination == null) return null!;

        return new Bank
        {
            Id = destination.Id,
            CompanyId = destination.CompanyId,
            BankName = destination.BankName,
            BankCode = destination.BankCode,
            SwiftCode = destination.SwiftCode,
            Address = destination.Address,
            City = destination.City,
            State = destination.State,
            Country = destination.Country,
            PostalCode = destination.PostalCode,
            Phone = destination.Phone,
            Email = destination.Email,
            Website = destination.Website,
            IsActive = destination.IsActive,
            TenantId = destination.TenantId != Guid.Empty ? destination.TenantId : Guid.Empty,
            CreatedAt = destination.CreatedAt,
            UpdatedAt = destination.UpdatedAt
        };
    }
}

