using ERP.Domain;

namespace ERP.Domain.CRM;

public class ReturnOrder : BaseEntity
{
    public string ReturnNumber { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? SalesOrderId { get; set; }
    public Guid? SalesInvoiceId { get; set; }
    public Guid? WarehouseId { get; set; }
    /// <summary>Sales credit note created/posted on Complete (not a Finance invoice id).</summary>
    public Guid? CreditNoteId { get; set; }
    public string Reason { get; set; } = string.Empty;
    /// <summary>Pending → Approved → Completed | Cancelled</summary>
    public string Status { get; set; } = "Pending";
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }

    public Customer? Customer { get; set; }
    public SalesOrder? SalesOrder { get; set; }
    public ICollection<ReturnOrderLine> Lines { get; set; } = new List<ReturnOrderLine>();
}
