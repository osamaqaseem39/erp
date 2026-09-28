using ERP.Domain;

namespace ERP.Domain.CRM;

public class ReturnOrder : BaseEntity
{
    public string ReturnNumber { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? SalesOrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }

    public Customer? Customer { get; set; }
    public SalesOrder? SalesOrder { get; set; }
}
