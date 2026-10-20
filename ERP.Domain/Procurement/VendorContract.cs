using ERP.Domain;

namespace ERP.Domain.Procurement;

public class VendorContract : BaseEntity
{
    public Guid VendorId { get; set; }
    public Guid? ItemId { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public Guid? PaymentTermId { get; set; }
    public string? ContractNumber { get; set; }

    public Vendor Vendor { get; set; } = null!;
    public SCM.Item? Item { get; set; }
}
