using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Procurement;

public class VendorDto : BaseDto
{
    public Guid CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public string VendorCode { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public string? LocalName { get; set; }
    public string? VendorType { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Status { get; set; } = "Active";
    public string? Phone2 { get; set; }
    public string? Cell { get; set; }
    public string? Cell2 { get; set; }
    public string? Fax { get; set; }
    public Guid? CityId { get; set; }
    public Guid? PaymentTermId { get; set; }
    public Guid? PayableAccountId { get; set; }
    public int? Rating { get; set; }
    public List<VendorBankAccountDto>? BankAccounts { get; set; }
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? TaxNumber { get; set; }
    public string? TaxType { get; set; }
    public string? Ntn { get; set; }
    public string? Strn { get; set; }
    public string? Cnic { get; set; }
    public string? Website { get; set; }
    public string? PaymentTerms { get; set; }
    public decimal? CreditLimit { get; set; }
    public int CreditDays { get; set; }
    public string? Category { get; set; }
    public string? Notes { get; set; }
    public bool IsApproved { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
}

public class CreateVendorDto : CreateDto
{
    public string VendorCode { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public string? LocalName { get; set; }
    public string? VendorType { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Phone2 { get; set; }
    public string? Cell { get; set; }
    public string? Cell2 { get; set; }
    public string? Fax { get; set; }
    public Guid? CityId { get; set; }
    public Guid? PaymentTermId { get; set; }
    public Guid? PayableAccountId { get; set; }
    public int? Rating { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? TaxNumber { get; set; }
    public string? TaxType { get; set; }
    public string? Ntn { get; set; }
    public string? Strn { get; set; }
    public string? Cnic { get; set; }
    public string? Website { get; set; }
    public string? PaymentTerms { get; set; }
    public decimal? CreditLimit { get; set; }
    public int CreditDays { get; set; }
    public string? Status { get; set; } = "Active";
    public string? Category { get; set; }
    public string? Notes { get; set; }
    public Guid CompanyId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateVendorDto : UpdateDto
{
    public string? VendorCode { get; set; }
    public string? VendorName { get; set; }
    public string? LocalName { get; set; }
    public string? VendorType { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Phone2 { get; set; }
    public string? Cell { get; set; }
    public string? Cell2 { get; set; }
    public string? Fax { get; set; }
    public Guid? CityId { get; set; }
    public Guid? PaymentTermId { get; set; }
    public Guid? PayableAccountId { get; set; }
    public int? Rating { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? TaxNumber { get; set; }
    public string? TaxType { get; set; }
    public string? Ntn { get; set; }
    public string? Strn { get; set; }
    public string? Cnic { get; set; }
    public string? Website { get; set; }
    public string? PaymentTerms { get; set; }
    public decimal? CreditLimit { get; set; }
    public int? CreditDays { get; set; }
    public string? Status { get; set; }
    public string? Category { get; set; }
    public string? Notes { get; set; }
    public bool? IsActive { get; set; }
    public Guid? VendorId { get; set; }
}

public class VendorQuery
{
    public Guid TenantId { get; set; }
    public Guid? CompanyId { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "ASC";
}


// Aliases for GraphQL compatibility
public class VendorCreateDto : CreateVendorDto { }
public class VendorUpdateDto : UpdateVendorDto { }