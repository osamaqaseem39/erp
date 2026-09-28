using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.CRM;

public class CustomerDto : BaseDto
{
    public Guid CompanyId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string CustomerType { get; set; } = "Individual";
    public string Status { get; set; } = "Active";
    public string? Industry { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal OutstandingBalance { get; set; }
    public string? PaymentTerms { get; set; }
    public string? TaxId { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public new bool IsActive { get; set; } = true;
}

public class CreateCustomerDto : CreateDto
{
    public Guid CompanyId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string CustomerType { get; set; } = "Individual";
    public string Status { get; set; } = "Active";
    public string? Industry { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal OutstandingBalance { get; set; }
    public string? PaymentTerms { get; set; }
    public string? TaxId { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }
    public string CurrencyCode { get; set; } = "USD";
}

public class UpdateCustomerDto : UpdateDto
{
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string CustomerType { get; set; } = "Individual";
    public string Status { get; set; } = "Active";
    public string? Industry { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal OutstandingBalance { get; set; }
    public string? PaymentTerms { get; set; }
    public string? TaxId { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }
    public string CurrencyCode { get; set; } = "USD";
}
public class CustomerPagedQuery : PagedRequest
{
    public string? Status { get; set; }
    public string? Industry { get; set; }
    public string? CustomerType { get; set; }
}
