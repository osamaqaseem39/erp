using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.CRM;

public class PouchTypeDto : BaseDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreatePouchTypeDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdatePouchTypeDto : CreatePouchTypeDto
{
    public Guid Id { get; set; }
}

public class ProductionProcessDto : BaseDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateProductionProcessDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateProductionProcessDto : CreateProductionProcessDto
{
    public Guid Id { get; set; }
}

public class FOHTypeDto : BaseDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Basis { get; set; }
    public decimal Rate { get; set; }
    public string? Description { get; set; }
}

public class CreateFOHTypeDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Basis { get; set; }
    public decimal Rate { get; set; }
    public string? Description { get; set; }
}

public class UpdateFOHTypeDto : CreateFOHTypeDto
{
    public Guid Id { get; set; }
}

public class PackagingLineSpecDto
{
    public string? Size { get; set; }
    public string? MaterialType { get; set; }
    public decimal? PET { get; set; }
    public decimal? MPET { get; set; }
    public decimal? LaminationMicron { get; set; }
    public decimal? CutOffLength { get; set; }
    public decimal? FolderSize { get; set; }
    public int? Ups { get; set; }
    public Guid? PouchTypeId { get; set; }
    public Guid? ProcessId { get; set; }
    public Guid? FOHTypeId { get; set; }
}

public class SaleEnquiryLineDto : PackagingLineSpecDto
{
    public Guid Id { get; set; }
    public Guid? ItemId { get; set; }
    public string ItemDescription { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}

public class CreateSaleEnquiryLineDto : PackagingLineSpecDto
{
    public Guid? ItemId { get; set; }
    public string ItemDescription { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}

public class SaleEnquiryDto : BaseDto
{
    public string EnquiryNumber { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public DateTime EnquiryDate { get; set; }
    public string Status { get; set; } = "Draft";
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
    public string? CustomerName { get; set; }
    public int LineCount { get; set; }
    public decimal TotalQuantity { get; set; }
    public string? CreatedBy { get; set; }
    public string? RejectionReason { get; set; }
    /// <summary>The pre-cost built from this enquiry, if any.</summary>
    public Guid? SalePreCostId { get; set; }
    public string? SalePreCostNumber { get; set; }
    public List<SaleEnquiryLineDto> Lines { get; set; } = new();
}

public class RejectSaleEnquiryDto
{
    public string Reason { get; set; } = string.Empty;
}

public class CreateSaleEnquiryDto
{
    public Guid? CustomerId { get; set; }
    public DateTime? EnquiryDate { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
    public List<CreateSaleEnquiryLineDto> Lines { get; set; } = new();
}

public class UpdateSaleEnquiryDto : CreateSaleEnquiryDto
{
    public Guid Id { get; set; }
}

public class SalePreCostLineDto : PackagingLineSpecDto
{
    public Guid Id { get; set; }
    public Guid? ItemId { get; set; }
    public string ItemDescription { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal MaterialCost { get; set; }
    public decimal ProcessCost { get; set; }
    public decimal FohCost { get; set; }
    public decimal MarginPercent { get; set; }
    public decimal UnitRate { get; set; }
    public string? Notes { get; set; }
}

public class CreateSalePreCostLineDto : PackagingLineSpecDto
{
    public Guid? ItemId { get; set; }
    public string ItemDescription { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal MaterialCost { get; set; }
    public decimal ProcessCost { get; set; }
    public decimal FohCost { get; set; }
    public decimal MarginPercent { get; set; }
    public decimal UnitRate { get; set; }
    public string? Notes { get; set; }
}

public class SalePreCostDto : BaseDto
{
    public string PreCostNumber { get; set; } = string.Empty;
    public Guid? SaleEnquiryId { get; set; }
    public Guid? QuoteId { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime PreCostDate { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Notes { get; set; }
    public List<SalePreCostLineDto> Lines { get; set; } = new();
}

public class CreateSalePreCostDto
{
    public Guid? SaleEnquiryId { get; set; }
    public Guid? QuoteId { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime? PreCostDate { get; set; }
    public string? Notes { get; set; }
    public List<CreateSalePreCostLineDto> Lines { get; set; } = new();
}

public class UpdateSalePreCostDto : CreateSalePreCostDto
{
    public Guid Id { get; set; }
}
