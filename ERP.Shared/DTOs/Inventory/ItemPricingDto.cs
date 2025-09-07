using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class ItemPricingDto : BaseDto
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid? PriceListId { get; set; }
    public string? PriceListName { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public int MinimumQuantity { get; set; }
    public int MaximumQuantity { get; set; }
    public decimal MinQuantity { get; set; }
    public decimal MaxQuantity { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}

public class CreateItemPricingDto : CreateDto
{
    public Guid ItemId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? PriceListId { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public int? MinimumQuantity { get; set; }
    public int? MaximumQuantity { get; set; }
    public decimal? MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
    public string? Notes { get; set; }
    public bool? IsActive { get; set; } = true;
}

public class UpdateItemPricingDto : UpdateDto
{
    public decimal? UnitPrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public int? MinimumQuantity { get; set; }
    public int? MaximumQuantity { get; set; }
    public decimal? MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
    public string? Notes { get; set; }
    public bool? IsActive { get; set; }
}