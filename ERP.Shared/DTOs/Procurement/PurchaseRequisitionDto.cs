using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Procurement;

public class PurchaseRequisitionDto : BaseDto
{
    public string RequisitionNumber { get; set; } = string.Empty;
    public Guid RequestedById { get; set; }
    public string? RequestedByName { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public Guid RequestedByUserId { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public DateTime? RequiredByDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Draft";
    public string Priority { get; set; } = "Medium";
    public string? Justification { get; set; }
    public string? Remarks { get; set; }
    public string? Notes { get; set; }
    public List<PurchaseRequisitionDetailDto>? RequisitionDetails { get; set; }
    public List<PurchaseRequisitionDetailDto>? Details { get; set; }
}

public class CreatePurchaseRequisitionDto : CreateDto
{
    public Guid? CompanyId { get; set; }
    public string? RequisitionNumber { get; set; }
    public Guid RequestedById { get; set; }
    public Guid? DepartmentId { get; set; }
    public DateTime? RequestDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Status { get; set; } = "Draft";
    public string? Priority { get; set; } = "Medium";
    public string? Justification { get; set; }
    public string? Notes { get; set; }
    public string? Remarks { get; set; }
    public List<CreatePurchaseRequisitionDetailDto> Details { get; set; } = new();
}

public class UpdatePurchaseRequisitionDto : UpdateDto
{
    public Guid? CompanyId { get; set; }
    public string? RequisitionNumber { get; set; }
    public Guid? RequestedById { get; set; }
    public Guid? DepartmentId { get; set; }
    public DateTime? RequestDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? Justification { get; set; }
    public string? Notes { get; set; }
    public string? Remarks { get; set; }
    public List<CreatePurchaseRequisitionDetailDto> Details { get; set; } = new();
}

public class PurchaseRequisitionDetailDto : BaseDto
{
    public Guid Id { get; set; }
    public Guid RequisitionId { get; set; }
    public Guid PRId { get; set; }
    public Guid ItemId { get; set; }
    public string? ItemName { get; set; }
    public string? ItemCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal QuantityRequested { get; set; }
    public decimal EstimatedUnitPrice { get; set; }
    public decimal EstimatedTotalPrice { get; set; }
    public Guid? UnitOfMeasureId { get; set; }
    public string? UnitOfMeasureName { get; set; }
    public string? UnitSymbol { get; set; }
    public string? Specifications { get; set; }
    public string? Remarks { get; set; }
    public string? Notes { get; set; }
}

public class CreatePurchaseRequisitionDetailDto
{
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public Guid? UnitOfMeasureId { get; set; }
    public decimal? EstimatedUnitPrice { get; set; }
    public string? Specifications { get; set; }
    public string? Notes { get; set; }
}

public class UpdatePurchaseRequisitionDetailDto : UpdateDto 
{
    public Guid? ItemId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? EstimatedUnitPrice { get; set; }
    public string? Specifications { get; set; }
    public string? Notes { get; set; }
}

// Aliases for GraphQL compatibility
public class PurchaseRequisitionCreateDto : CreatePurchaseRequisitionDto { }
public class PurchaseRequisitionUpdateDto : UpdatePurchaseRequisitionDto { }