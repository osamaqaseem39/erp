using ERP.Domain;
using ERP.Domain.Core;

namespace ERP.Domain.SCM;

public class StockLedger : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid ItemId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? WarehouseLocationId { get; set; }
    public string TransactionType { get; set; } = string.Empty; // Receipt, Issue, Transfer, Adjustment, Return
    public DateTime TransactionDate { get; set; }
    public decimal QuantityIn { get; set; }
    public decimal QuantityOut { get; set; }
    public decimal BalanceQty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalValue { get; set; }
    public string? ReferenceDocType { get; set; } // PO, SO, Invoice, Return
    public Guid? ReferenceDocId { get; set; }

    public virtual Company Company { get; set; } = null!;
    public virtual Item Item { get; set; } = null!;
    public virtual Warehouse Warehouse { get; set; } = null!;
    public virtual WarehouseLocation? WarehouseLocation { get; set; }
}
