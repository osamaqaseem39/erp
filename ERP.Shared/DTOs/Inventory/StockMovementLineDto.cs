namespace ERP.Shared.DTOs.Inventory;

/// <summary>GraphQL/REST-friendly stock movement line (maps to service tuple APIs).</summary>
public class StockMovementLineDto
{
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal? UnitCost { get; set; }
}
