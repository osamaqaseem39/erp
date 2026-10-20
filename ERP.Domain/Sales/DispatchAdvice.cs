using ERP.Domain;
using ERP.Domain.CRM;

namespace ERP.Domain.Sales;

public class DispatchAdvice : BaseEntity
{
    public string AdviceNumber { get; set; } = string.Empty;
    public Guid? SalesOrderId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? WarehouseId { get; set; }
    public DateTime AdviceDate { get; set; }
    /// <summary>Draft → Approved → Dispatched | Cancelled</summary>
    public string Status { get; set; } = "Draft";
    public DateTime? DispatchedAt { get; set; }
    public string? Notes { get; set; }

    public SalesOrder? SalesOrder { get; set; }
    public Customer? Customer { get; set; }
    public ICollection<DispatchAdviceLine> Lines { get; set; } = new List<DispatchAdviceLine>();
}
