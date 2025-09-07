using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Procurement;

public class VendorBankAccountDto : BaseDto
{
    public Guid VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string RoutingNumber { get; set; } = string.Empty;
    public string SwiftCode { get; set; } = string.Empty;
    public string? IBAN { get; set; }
    public string? SWIFTCode { get; set; }
    public string? AccountHolderName { get; set; }
    public string? CurrencyCode { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsPrimary { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}

public class CreateVendorBankAccountDto : CreateDto
{
    public Guid VendorId { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string RoutingNumber { get; set; } = string.Empty;
    public string SwiftCode { get; set; } = string.Empty;
    public string? IBAN { get; set; }
    public string? SWIFTCode { get; set; }
    public string? AccountHolderName { get; set; }
    public string? CurrencyCode { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsPrimary { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}

public class UpdateVendorBankAccountDto : UpdateDto
{
    public string? BankName { get; set; }
    public string? AccountName { get; set; }
    public string? AccountNumber { get; set; }
    public string? RoutingNumber { get; set; }
    public string? SwiftCode { get; set; }
    public string? IBAN { get; set; }
    public string? SWIFTCode { get; set; }
    public string? AccountHolderName { get; set; }
    public string? CurrencyCode { get; set; }
    public string? Currency { get; set; }
    public bool? IsPrimary { get; set; }
    public bool? IsActive { get; set; }
    public string? Notes { get; set; }
}