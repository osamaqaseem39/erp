using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance;

public class TaxCodeDto : BaseDto
{
    public string TaxCode { get; set; } = string.Empty;
    public string TaxName { get; set; } = string.Empty;
    public string TaxType { get; set; } = string.Empty; // Sales, Purchase, VAT, GST
    public decimal TaxRate { get; set; }
    public Guid? GLAccountId { get; set; }
    public string? GLAccountName { get; set; }
    public bool IsActive { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Description { get; set; }
}

public class CreateTaxCodeDto : CreateDto
{
    public string TaxCode { get; set; } = string.Empty;
    public string TaxName { get; set; } = string.Empty;
    public string TaxType { get; set; } = string.Empty;
    public decimal? TaxRate { get; set; }
    public Guid? GLAccountId { get; set; }
    public bool? IsActive { get; set; } = true;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Description { get; set; }
}

public class UpdateTaxCodeDto : UpdateDto
{
    public string? TaxCode { get; set; }
    public string? TaxName { get; set; }
    public string? TaxType { get; set; }
    public decimal? TaxRate { get; set; }
    public Guid? GLAccountId { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Description { get; set; }
}