using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.CRM;

public class SalesmanDto : BaseDto
{
    public string SalesmanCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Cnic { get; set; }
    public string? Remarks { get; set; }
    public string Status { get; set; } = "Active";
}

public class CreateSalesmanDto : CreateDto
{
    public string? SalesmanCode { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Cnic { get; set; }
    public string? Remarks { get; set; }
    public string Status { get; set; } = "Active";
}

public class UpdateSalesmanDto : UpdateDto
{
    public string? SalesmanCode { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Cnic { get; set; }
    public string? Remarks { get; set; }
    public string? Status { get; set; }
}

public class CustomerGroupDto : BaseDto
{
    public string GroupName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DiscountPercentage { get; set; }
}

public class CreateCustomerGroupDto : CreateDto
{
    public string GroupName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DiscountPercentage { get; set; }
}

public class UpdateCustomerGroupDto : UpdateDto
{
    public string? GroupName { get; set; }
    public string? Description { get; set; }
    public decimal? DiscountPercentage { get; set; }
}

public class PriceListDto : BaseDto
{
    public string PriceListName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsDefault { get; set; }
}

public class CreatePriceListDto : CreateDto
{
    public string PriceListName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsDefault { get; set; }
}

public class UpdatePriceListDto : UpdateDto
{
    public string? PriceListName { get; set; }
    public string? Description { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool? IsDefault { get; set; }
}

public class SalesTerritoryDto : BaseDto
{
    public string TerritoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ManagerId { get; set; }
}

public class CreateSalesTerritoryDto : CreateDto
{
    public string TerritoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ManagerId { get; set; }
}

public class UpdateSalesTerritoryDto : UpdateDto
{
    public string? TerritoryName { get; set; }
    public string? Description { get; set; }
    public Guid? ManagerId { get; set; }
}

public class ReturnOrderLineDto
{
    public Guid Id { get; set; }
    public Guid ReturnOrderId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string? Reason { get; set; }
}

public class CreateReturnOrderLineDto
{
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Reason { get; set; }
}

public class ReturnOrderDto : BaseDto
{
    public string ReturnNumber { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid? SalesOrderId { get; set; }
    public Guid? SalesInvoiceId { get; set; }
    public Guid? WarehouseId { get; set; }
    public Guid? CreditNoteId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public List<ReturnOrderLineDto> Lines { get; set; } = new();
}

public class CreateReturnOrderDto : CreateDto
{
    public string? ReturnNumber { get; set; }
    public DateTime ReturnDate { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? SalesOrderId { get; set; }
    public Guid? SalesInvoiceId { get; set; }
    public Guid? WarehouseId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public List<CreateReturnOrderLineDto> Lines { get; set; } = new();
}

public class UpdateReturnOrderDto : UpdateDto
{
    public string? Reason { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Notes { get; set; }
    public Guid? WarehouseId { get; set; }
    public Guid? SalesInvoiceId { get; set; }
    public List<CreateReturnOrderLineDto>? Lines { get; set; }
}
