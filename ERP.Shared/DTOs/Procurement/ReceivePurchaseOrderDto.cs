using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Procurement;

public class ReceivePurchaseOrderDto
{
    public Guid PurchaseOrderId { get; set; }
    public DateTime ReceivedDate { get; set; }
    public string? Notes { get; set; }
    public List<ReceivePurchaseOrderLineDto> Lines { get; set; } = new();
}

public class ReceivePurchaseOrderLineDto
{
    public Guid PurchaseOrderLineId { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public string? Notes { get; set; }
}