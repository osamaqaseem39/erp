using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Procurement;

public class RequestForQuotationDto : BaseDto
{
    public string RFQNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ResponseDate { get; set; }
    public string Status { get; set; } = "Draft";
    public string Priority { get; set; } = "Medium";
    public decimal? EstimatedValue { get; set; }
    public string? Terms { get; set; }
    public string? Notes { get; set; }
    public List<RFQDetailDto>? RFQDetails { get; set; }
    public List<RFQVendorResponseDto>? VendorResponses { get; set; }
}

public class CreateRequestForQuotationDto : CreateDto
{
    public string? RFQNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Status { get; set; } = "Draft";
    public string? Priority { get; set; } = "Medium";
    public decimal? EstimatedValue { get; set; }
    public string? Terms { get; set; }
    public string? Notes { get; set; }
}

public class UpdateRequestForQuotationDto : UpdateDto
{
    public string? RFQNumber { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ResponseDate { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public decimal? EstimatedValue { get; set; }
    public string? Terms { get; set; }
    public string? Notes { get; set; }
}

public class RFQDetailDto
{
    public Guid Id { get; set; }
    public Guid RFQId { get; set; }
    public Guid ItemId { get; set; }
    public string? ItemName { get; set; }
    public string? ItemCode { get; set; }
    public decimal Quantity { get; set; }
    public string? Specifications { get; set; }
    public string? Notes { get; set; }
}

public class CreateRFQDetailDto
{
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public string? Specifications { get; set; }
    public string? Notes { get; set; }
}

public class UpdateRFQDetailDto
{
    public Guid? ItemId { get; set; }
    public decimal? Quantity { get; set; }
    public string? Specifications { get; set; }
    public string? Notes { get; set; }
}

public class RFQVendorResponseDto
{
    public Guid Id { get; set; }
    public Guid RFQId { get; set; }
    public Guid VendorId { get; set; }
    public string? VendorName { get; set; }
    public DateTime ResponseDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Submitted";
    public string? Notes { get; set; }
    public List<RFQVendorResponseDetailDto>? ResponseDetails { get; set; }
}

public class CreateRFQVendorResponseDto : CreateDto
{
    public Guid RFQId { get; set; }
    public Guid VendorId { get; set; }
    public DateTime? ResponseDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Status { get; set; } = "Submitted";
    public string? Notes { get; set; }
}

public class UpdateRFQVendorResponseDto : UpdateDto
{
    public DateTime? ResponseDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
}

public class RFQVendorResponseDetailDto
{
    public Guid Id { get; set; }
    public Guid ResponseId { get; set; }
    public Guid RFQDetailId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Specifications { get; set; }
    public string? Notes { get; set; }
}

public class CreateRFQVendorResponseDetailDto
{
    public Guid RFQDetailId { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Specifications { get; set; }
    public string? Notes { get; set; }
}

public class UpdateRFQVendorResponseDetailDto
{
    public decimal? UnitPrice { get; set; }
    public string? Specifications { get; set; }
    public string? Notes { get; set; }
}