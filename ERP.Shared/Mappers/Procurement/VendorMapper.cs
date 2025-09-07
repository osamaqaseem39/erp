using ERP.Domain.Procurement;
using ERP.Shared.DTOs.Procurement;

namespace ERP.Shared.Mappers.Procurement;

public class VendorMapper : IMapper<Vendor, VendorDto>
{
    public VendorDto ToDto(Vendor entity)
    {
        return new VendorDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CompanyId = entity.CompanyId,
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
            UpdatedBy = entity.UpdatedBy
        };
    }

    public Vendor ToEntity(VendorDto dto)
    {
        return new Vendor
        {
            Id = dto.Id,
            TenantId = dto.TenantId,
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
            CreditLimit = dto.CreditLimit ?? 0m,
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            CreatedBy = dto.CreatedBy,
            UpdatedBy = dto.UpdatedBy
        };
    }

    public Vendor MapToEntity(CreateVendorDto dto, Guid tenantId, string createdBy)
    {
        return new Vendor
        {
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
            CreditLimit = dto.CreditLimit ?? 0m,
            IsActive = dto.IsActive,
            CreatedBy = createdBy
        };
    }

    public Vendor MapToEntity(UpdateVendorDto dto, Vendor existingVendor, string updatedBy)
    {
        if (dto.VendorCode != null) existingVendor.VendorCode = dto.VendorCode;
        if (dto.VendorName != null) existingVendor.VendorName = dto.VendorName;
        if (dto.ContactPerson != null) existingVendor.ContactPerson = dto.ContactPerson;
        if (dto.PhoneNumber != null) existingVendor.PhoneNumber = dto.PhoneNumber;
        if (dto.Email != null) existingVendor.Email = dto.Email;
        if (dto.AddressLine1 != null) existingVendor.AddressLine1 = dto.AddressLine1;
        if (dto.AddressLine2 != null) existingVendor.AddressLine2 = dto.AddressLine2;
        if (dto.City != null) existingVendor.City = dto.City;
        if (dto.State != null) existingVendor.State = dto.State;
        if (dto.Country != null) existingVendor.Country = dto.Country;
        if (dto.PostalCode != null) existingVendor.PostalCode = dto.PostalCode;
        if (dto.TaxNumber != null) existingVendor.TaxNumber = dto.TaxNumber;
        if (dto.PaymentTerms != null) existingVendor.PaymentTerms = dto.PaymentTerms;
        if (dto.CreditLimit.HasValue) existingVendor.CreditLimit = dto.CreditLimit.Value;
        if (dto.IsActive.HasValue) existingVendor.IsActive = dto.IsActive.Value;
        existingVendor.UpdatedBy = updatedBy;
        existingVendor.UpdatedAt = DateTime.UtcNow;
        return existingVendor;
    }

    public VendorDto MapToDto(Vendor entity)
    {
        return ToDto(entity);
    }

    public VendorBankAccount MapToBankAccountEntity(CreateVendorBankAccountDto dto, Guid vendorId, Guid tenantId, string createdBy)
    {
        return new VendorBankAccount
        {
            VendorId = vendorId,
            TenantId = tenantId,
            BankName = dto.BankName,
            AccountNumber = dto.AccountNumber,
            IBAN = dto.IBAN,
            SWIFTCode = dto.SwiftCode,
            Currency = dto.Currency,
            IsPrimary = dto.IsPrimary,
            IsActive = true,
            CreatedBy = createdBy
        };
    }
}