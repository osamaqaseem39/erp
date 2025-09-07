using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory
{
    public class ItemDto : BaseDto
    {
        public Guid CompanyId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid ItemCategoryId { get; set; }
        public string ItemCategoryName { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public Guid UnitOfMeasureId { get; set; }
        public string UnitOfMeasureName { get; set; } = string.Empty;
        public string HSNCode { get; set; } = string.Empty;
        public string BarCode { get; set; } = string.Empty;
        public Guid? DefaultWarehouseId { get; set; }
        public string DefaultWarehouseName { get; set; } = string.Empty;
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal MinimumStock { get; set; }
        public decimal MaximumStock { get; set; }
        public decimal ReorderLevel { get; set; }
        public decimal ReorderQuantity { get; set; }
        public bool IsSerialized { get; set; }
        public bool IsBatchTracked { get; set; }
        public new bool IsActive { get; set; }
    }

    public class CreateItemDto : CreateDto
    {
        public Guid CompanyId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid ItemCategoryId { get; set; }
        public string CategoryId { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public Guid UnitOfMeasureId { get; set; }
        public string HSNCode { get; set; } = string.Empty;
        public string BarCode { get; set; } = string.Empty;
        public Guid? DefaultWarehouseId { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal MinimumStock { get; set; }
        public decimal MaximumStock { get; set; }
        public decimal ReorderLevel { get; set; }
        public decimal ReorderQuantity { get; set; }
        public bool IsSerialized { get; set; }
        public bool IsBatchTracked { get; set; }
        public new bool IsActive { get; set; } = true;
    }

    public class UpdateItemDto : UpdateDto
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid ItemCategoryId { get; set; }
        public string CategoryId { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public Guid UnitOfMeasureId { get; set; }
        public string HSNCode { get; set; } = string.Empty;
        public string BarCode { get; set; } = string.Empty;
        public Guid? DefaultWarehouseId { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal MinimumStock { get; set; }
        public decimal MaximumStock { get; set; }
        public decimal ReorderLevel { get; set; }
        public decimal ReorderQuantity { get; set; }
        public bool IsSerialized { get; set; }
        public bool IsBatchTracked { get; set; }
        public new bool IsActive { get; set; }
    }
}