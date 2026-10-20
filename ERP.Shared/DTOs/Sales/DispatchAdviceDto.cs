using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Sales;

public class DispatchAdviceLineDto
{
    public Guid Id { get; set; }
    public Guid? SalesOrderLineId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}

public class CreateDispatchAdviceLineDto
{
    public Guid? SalesOrderLineId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}

public class DispatchAdviceDto : BaseDto
{
    public string AdviceNumber { get; set; } = string.Empty;
    public Guid? SalesOrderId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? WarehouseId { get; set; }
    public DateTime AdviceDate { get; set; }
    public string Status { get; set; } = "Draft";
    public DateTime? DispatchedAt { get; set; }
    public string? Notes { get; set; }
    public List<DispatchAdviceLineDto> Lines { get; set; } = new();
}

public class CreateDispatchAdviceDto
{
    public Guid? SalesOrderId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? WarehouseId { get; set; }
    public DateTime? AdviceDate { get; set; }
    public string? Notes { get; set; }
    public List<CreateDispatchAdviceLineDto> Lines { get; set; } = new();
}

public class UpdateDispatchAdviceDto : CreateDispatchAdviceDto
{
    public Guid Id { get; set; }
}
