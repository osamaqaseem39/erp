using ERP.Domain;

namespace ERP.Domain.Procurement;

public class LandedCostVoucher : BaseEntity
{
    public string VoucherNumber { get; set; } = string.Empty;
    public Guid? VendorId { get; set; }
    public DateTime VoucherDate { get; set; }
    public string? Description { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Draft";
    public DateTime? PostedAt { get; set; }
    public string? PostedBy { get; set; }

    public Vendor? Vendor { get; set; }
    public ICollection<LandedCostVoucherLine> Lines { get; set; } = new List<LandedCostVoucherLine>();
}

public class LandedCostVoucherLine : BaseEntity
{
    public Guid LandedCostVoucherId { get; set; }
    public Guid? ItemId { get; set; }
    public Guid? GrnDetailId { get; set; }
    public Guid? PurchaseInvoiceDetailId { get; set; }
    public decimal AllocatedAmount { get; set; }
    public string? Notes { get; set; }

    public LandedCostVoucher LandedCostVoucher { get; set; } = null!;
}
