using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Core;

public class CompanyDto : BaseDto
{
    public string CompanyCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public string TimeZone { get; set; } = string.Empty;
    public new bool IsActive { get; set; } = true;
}

public class CreateCompanyDto : CreateDto
{
    public string? CompanyCode { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? TaxNumber { get; set; }
    public string? TaxId { get; set; }
    public string? Address { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? Currency { get; set; } = "USD";
    public string? TimeZone { get; set; }
    public bool? IsActive { get; set; } = true;
}

public class UpdateCompanyDto : UpdateDto
{
    public string? CompanyCode { get; set; }
    public string? CompanyName { get; set; }
    public string? LegalName { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? TaxNumber { get; set; }
    public string? TaxId { get; set; }
    public string? Address { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }
    public string? PostalCode { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string? Currency { get; set; }
    public string? TimeZone { get; set; }
    public new bool IsActive { get; set; } = true;
}