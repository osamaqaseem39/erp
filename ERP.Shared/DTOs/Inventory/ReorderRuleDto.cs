using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class ReorderRuleDto : BaseDto
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public decimal MinimumStock { get; set; }
    public decimal MaximumStock { get; set; }
    public decimal MinStockLevel { get; set; }
    public decimal MaxStockLevel { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal ReorderPoint { get; set; }
    public decimal ReorderQuantity { get; set; }
    public int LeadTimeDays { get; set; }
    public string RuleType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateReorderRuleDto : CreateDto
{
    public Guid ItemId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal MinimumStock { get; set; }
    public decimal MaximumStock { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal ReorderQuantity { get; set; }
    public string RuleType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class UpdateReorderRuleDto : UpdateDto
{
    public decimal MinimumStock { get; set; }
    public decimal MaximumStock { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal ReorderQuantity { get; set; }
    public string RuleType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}