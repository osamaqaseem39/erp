using ERP.Domain.Core;
using ERP.Shared.DTOs.Core;
using ERP.Shared.Mappers;

namespace ERP.Shared.Mappers.Core;

public static class CompanyMapper
{
    public static CompanyDto ToDto(Company entity)
    {
        return new CompanyDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CompanyName = entity.CompanyName,
            CompanyCode = entity.CompanyCode,
            LegalName = entity.LegalName,
            TaxId = entity.TaxId,
            RegistrationNumber = entity.RegistrationNumber,
            Address = entity.Address,
            City = entity.City,
            State = entity.State,
            Country = entity.Country,
            PostalCode = entity.PostalCode,
            Phone = entity.Phone,
            Email = entity.Email,
            Website = entity.Website,
            Currency = entity.Currency,
            TimeZone = entity.TimeZone,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static Company ToEntity(CreateCompanyDto dto, Guid tenantId, string createdBy)
    {
        return new Company
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CompanyName = dto.CompanyName,
            CompanyCode = dto.CompanyCode ?? string.Empty,
            LegalName = dto.LegalName ?? string.Empty,
            TaxId = dto.TaxId ?? string.Empty,
            TaxNumber = dto.TaxId ?? string.Empty,
            RegistrationNumber = dto.RegistrationNumber ?? string.Empty,
            Address = dto.Address ?? string.Empty,
            AddressLine1 = dto.Address ?? string.Empty,
            City = dto.City ?? string.Empty,
            State = dto.State ?? string.Empty,
            Country = dto.Country ?? string.Empty,
            PostalCode = dto.PostalCode ?? string.Empty,
            ZipCode = dto.PostalCode ?? string.Empty,
            Phone = dto.Phone ?? string.Empty,
            Email = dto.Email ?? string.Empty,
            Website = dto.Website ?? string.Empty,
            Currency = dto.Currency ?? "USD",
            TimeZone = dto.TimeZone ?? "UTC",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static void UpdateEntity(UpdateCompanyDto dto, Company entity, string updatedBy)
    {
        if (!string.IsNullOrEmpty(dto.CompanyName)) entity.CompanyName = dto.CompanyName;
        if (!string.IsNullOrEmpty(dto.CompanyCode)) entity.CompanyCode = dto.CompanyCode;
        if (!string.IsNullOrEmpty(dto.LegalName)) entity.LegalName = dto.LegalName;
        if (!string.IsNullOrEmpty(dto.TaxId)) 
        {
            entity.TaxId = dto.TaxId;
            entity.TaxNumber = dto.TaxId;
        }
        if (!string.IsNullOrEmpty(dto.RegistrationNumber)) entity.RegistrationNumber = dto.RegistrationNumber;
        if (!string.IsNullOrEmpty(dto.Address)) 
        {
            entity.Address = dto.Address;
            entity.AddressLine1 = dto.Address;
        }
        if (!string.IsNullOrEmpty(dto.City)) entity.City = dto.City;
        if (!string.IsNullOrEmpty(dto.State)) entity.State = dto.State;
        if (!string.IsNullOrEmpty(dto.Country)) entity.Country = dto.Country;
        if (!string.IsNullOrEmpty(dto.PostalCode)) 
        {
            entity.PostalCode = dto.PostalCode;
            entity.ZipCode = dto.PostalCode;
        }
        if (!string.IsNullOrEmpty(dto.Phone)) entity.Phone = dto.Phone;
        if (!string.IsNullOrEmpty(dto.Email)) entity.Email = dto.Email;
        if (!string.IsNullOrEmpty(dto.Website)) entity.Website = dto.Website;
        if (!string.IsNullOrEmpty(dto.Currency)) entity.Currency = dto.Currency;
        if (!string.IsNullOrEmpty(dto.TimeZone)) entity.TimeZone = dto.TimeZone;
        entity.IsActive = dto.IsActive;
        
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;
    }
}