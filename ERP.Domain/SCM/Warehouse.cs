using ERP.Domain;
using ERP.Domain.Core;

namespace ERP.Domain.SCM;

public class Warehouse : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public Guid? ManagerId { get; set; }
    public string? Manager { get; set; }
    
    // Navigation properties
    public virtual Company Company { get; set; } = null!;
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
    public virtual ICollection<StockLedger> StockLedgers { get; set; } = new List<StockLedger>();
    public virtual ICollection<StockBalance> StockBalances { get; set; } = new List<StockBalance>();
    public virtual ICollection<BatchSerialTracking> BatchSerialTrackings { get; set; } = new List<BatchSerialTracking>();
    public virtual ICollection<InventoryAdjustment> InventoryAdjustments { get; set; } = new List<InventoryAdjustment>();
    public virtual ICollection<StockTransfer> FromTransfers { get; set; } = new List<StockTransfer>();
    public virtual ICollection<StockTransfer> ToTransfers { get; set; } = new List<StockTransfer>();
}
