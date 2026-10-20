using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Manufacturing;

public class BillOfMaterialDto : BaseDto
{
    public string BomNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid FinishedGoodItemId { get; set; }
    public string Version { get; set; } = "1";
    public string Status { get; set; } = "Draft";
    public bool IsDefault { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Notes { get; set; }
    public List<BillOfMaterialLineDto> Lines { get; set; } = [];
}

public class BillOfMaterialLineDto
{
    public Guid? Id { get; set; }
    public int LineNumber { get; set; }
    public Guid ComponentItemId { get; set; }
    public decimal QuantityPer { get; set; }
    public decimal ScrapPercent { get; set; }
    public string? AlternateGroup { get; set; }
    public string? Notes { get; set; }
}

public class CreateBillOfMaterialDto : CreateDto
{
    public string? BomNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid FinishedGoodItemId { get; set; }
    public string Version { get; set; } = "1";
    public bool IsDefault { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Notes { get; set; }
    public List<BillOfMaterialLineDto> Lines { get; set; } = [];
}

public class UpdateBillOfMaterialDto : UpdateDto
{
    public string? BomNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid FinishedGoodItemId { get; set; }
    public string Version { get; set; } = "1";
    public bool? IsDefault { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Notes { get; set; }
    public List<BillOfMaterialLineDto> Lines { get; set; } = [];
}
