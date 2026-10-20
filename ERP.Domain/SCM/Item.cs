using ERP.Domain;
using ERP.Domain.Core;

namespace ERP.Domain.SCM;

public class Item : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public string ItemType { get; set; } = string.Empty; // Raw, Finished, Service, Consumable
    public string? HSNCode { get; set; }
    public string? BarCode { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal ReorderQuantity { get; set; }
    public Guid? DefaultWarehouseId { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public bool IsSerialized { get; set; }
    public bool IsBatchTracked { get; set; }
    
    // Navigation properties
    public virtual Company Company { get; set; } = null!;
    public virtual ItemCategory Category { get; set; } = null!;
    public virtual Brand? Brand { get; set; }
    public virtual UnitOfMeasure UnitOfMeasure { get; set; } = null!;
    public virtual Warehouse? DefaultWarehouse { get; set; }
    public virtual ICollection<StockLedger> StockLedgers { get; set; } = new List<StockLedger>();
    public virtual ICollection<StockBalance> StockBalances { get; set; } = new List<StockBalance>();
    public virtual ICollection<BatchSerialTracking> BatchSerialTrackings { get; set; } = new List<BatchSerialTracking>();
    public virtual ICollection<InventoryAdjustment> InventoryAdjustments { get; set; } = new List<InventoryAdjustment>();
    public virtual ICollection<StockTransferDetail> StockTransferDetails { get; set; } = new List<StockTransferDetail>();
}
