using ERP.Domain.Procurement;
using ERP.Shared.DTOs.Procurement;

namespace ERP.Api.Mappers.Procurement;

public class VendorMapper
{
    public Vendor MapToEntity(CreateVendorDto dto, Guid tenantId, string createdBy)
    {
        return new Vendor
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CompanyId = dto.CompanyId,
            VendorCode = dto.VendorCode,
            VendorName = dto.VendorName,
            ContactPerson = dto.ContactPerson,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            AddressLine1 = dto.AddressLine1,
            AddressLine2 = dto.AddressLine2,
            City = dto.City,
            State = dto.State,
            Country = dto.Country,
            PostalCode = dto.PostalCode,
            TaxNumber = dto.TaxNumber,
            PaymentTerms = dto.PaymentTerms,
            CreditLimit = dto.CreditLimit ?? 0,
            CreatedBy = createdBy
        };
    }

    public void MapToEntity(UpdateVendorDto dto, Vendor entity, string updatedBy)
    {
        entity.VendorCode = dto.VendorCode;
        entity.VendorName = dto.VendorName;
        entity.ContactPerson = dto.ContactPerson;
        entity.PhoneNumber = dto.PhoneNumber;
        entity.Email = dto.Email;
        entity.AddressLine1 = dto.AddressLine1;
        entity.AddressLine2 = dto.AddressLine2;
        entity.City = dto.City;
        entity.State = dto.State;
        entity.Country = dto.Country;
        entity.PostalCode = dto.PostalCode;
        entity.TaxNumber = dto.TaxNumber;
        entity.PaymentTerms = dto.PaymentTerms;
        entity.CreditLimit = dto.CreditLimit ?? 0;
        entity.IsActive = dto.IsActive ?? true;
        entity.UpdatedBy = updatedBy;
        entity.UpdatedAt = DateTime.UtcNow;
    }

    public VendorDto MapToDto(Vendor entity)
    {
        return new VendorDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CompanyId = entity.CompanyId,
            CompanyName = entity.Company?.CompanyName ?? string.Empty,
            VendorCode = entity.VendorCode,
            VendorName = entity.VendorName,
            ContactPerson = entity.ContactPerson,
            PhoneNumber = entity.PhoneNumber,
            Email = entity.Email,
            AddressLine1 = entity.AddressLine1,
            AddressLine2 = entity.AddressLine2,
            City = entity.City,
            State = entity.State,
            Country = entity.Country,
            PostalCode = entity.PostalCode,
            TaxNumber = entity.TaxNumber,
            PaymentTerms = entity.PaymentTerms,
            CreditLimit = entity.CreditLimit,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            BankAccounts = entity.BankAccounts?.Select(MapToBankAccountDto).ToList() ?? new List<VendorBankAccountDto>()
        };
    }

    public VendorBankAccount MapToBankAccountEntity(CreateVendorBankAccountDto dto, Guid vendorId, Guid tenantId, string createdBy)
    {
        return new VendorBankAccount
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            VendorId = vendorId,
            BankName = dto.BankName,
            AccountNumber = dto.AccountNumber,
            IBAN = dto.IBAN,
            SWIFTCode = dto.SWIFTCode,
            Currency = dto.Currency,
            IsPrimary = dto.IsPrimary,
            CreatedBy = createdBy
        };
    }

    public VendorBankAccountDto MapToBankAccountDto(VendorBankAccount entity)
    {
        return new VendorBankAccountDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            VendorId = entity.VendorId,
            BankName = entity.BankName,
            AccountNumber = entity.AccountNumber,
            SwiftCode = entity.SWIFTCode,
            Currency = entity.Currency,
            IsPrimary = entity.IsPrimary,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy
        };
    }
}