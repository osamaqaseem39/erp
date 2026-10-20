using ERP.Domain;

namespace ERP.Domain.Finance;

public class CustomerAdvance : BaseEntity
{
    public string AdvanceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public DateTime AdvanceDate { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string Status { get; set; } = "Open";
    public Guid? PaymentId { get; set; }
    public string? Notes { get; set; }
}

public class VendorAdvance : BaseEntity
{
    public string AdvanceNumber { get; set; } = string.Empty;
    public Guid VendorId { get; set; }
    public DateTime AdvanceDate { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string Status { get; set; } = "Open";
    public Guid? PaymentId { get; set; }
    public string? Notes { get; set; }
}

public class AdvanceApplication : BaseEntity
{
    public Guid? CustomerAdvanceId { get; set; }
    public Guid? VendorAdvanceId { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? BillId { get; set; }
    public decimal AppliedAmount { get; set; }
    public DateTime AppliedDate { get; set; }
    public string Status { get; set; } = "Applied";
}
