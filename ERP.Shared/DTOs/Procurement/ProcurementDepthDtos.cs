using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Procurement;

public class LandedCostVoucherLineDto : BaseDto
{
    public Guid LandedCostVoucherId { get; set; }
    public Guid? ItemId { get; set; }
    public Guid? GrnDetailId { get; set; }
    public Guid? PurchaseInvoiceDetailId { get; set; }
    public decimal AllocatedAmount { get; set; }
    public string? Notes { get; set; }
}

public class LandedCostVoucherDto : BaseDto
{
    public string VoucherNumber { get; set; } = string.Empty;
    public Guid? VendorId { get; set; }
    public DateTime VoucherDate { get; set; }
    public string? Description { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Draft";
    public List<LandedCostVoucherLineDto> Lines { get; set; } = [];
}

public class CreateLandedCostVoucherLineDto
{
    public Guid? ItemId { get; set; }
    public Guid? GrnDetailId { get; set; }
    public Guid? PurchaseInvoiceDetailId { get; set; }
    public decimal AllocatedAmount { get; set; }
    public string? Notes { get; set; }
}

public class CreateLandedCostVoucherDto
{
    public Guid? VendorId { get; set; }
    public DateTime VoucherDate { get; set; }
    public string? VoucherNumber { get; set; }
    public string? Description { get; set; }
    public List<CreateLandedCostVoucherLineDto> Lines { get; set; } = [];
}

public class UpdateLandedCostVoucherDto : CreateLandedCostVoucherDto
{
    public Guid Id { get; set; }
}

public class VendorContractDto : BaseDto
{
    public Guid VendorId { get; set; }
    public Guid? ItemId { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public Guid? PaymentTermId { get; set; }
    public string? ContractNumber { get; set; }
}

public class CreateVendorContractDto
{
    public Guid VendorId { get; set; }
    public Guid? ItemId { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public Guid? PaymentTermId { get; set; }
    public string? ContractNumber { get; set; }
}

public class UpdateVendorContractDto : CreateVendorContractDto
{
    public Guid Id { get; set; }
    public bool? IsActive { get; set; }
}

public class ThreeWayMatchVarianceDto
{
    public Guid ItemId { get; set; }
    public decimal InvoiceQty { get; set; }
    public decimal PoQty { get; set; }
    public decimal GrnQty { get; set; }
    public decimal InvoicePrice { get; set; }
    public decimal PoPrice { get; set; }
    public string Message { get; set; } = string.Empty;
}
