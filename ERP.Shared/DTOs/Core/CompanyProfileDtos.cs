namespace ERP.Shared.DTOs.Core;

/// <summary>
/// The signed-in user's company, as one clean shape. The Company table has duplicated columns
/// (TaxNumber/TaxId, Address/AddressLine1, ZipCode/PostalCode); the service keeps both in sync.
/// </summary>
public class CompanyProfileDto
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? TaxId { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public string? MobilePhone { get; set; }
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    /// <summary>ISO 4217 code, e.g. USD, PKR.</summary>
    public string Currency { get; set; } = "USD";
    /// <summary>IANA time zone id, e.g. Asia/Karachi.</summary>
    public string TimeZone { get; set; } = "UTC";
}

public class UpdateCompanyProfileDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? TaxId { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public string? MobilePhone { get; set; }
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string Currency { get; set; } = "USD";
    public string TimeZone { get; set; } = "UTC";
}

/// <summary>What the app shell needs after sign-in to format money and dates for this tenant.</summary>
public class TenantContextDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public string TimeZone { get; set; } = "UTC";
    public string Locale { get; set; } = "en-US";
    public string DateFormat { get; set; } = "dd MMM yyyy";
    public int FirstDayOfWeek { get; set; } = 1;
}
