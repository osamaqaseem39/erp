using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance
{
    public class TaxRateDto : BaseDto
    {
        public string TaxCode { get; set; } = string.Empty;
        public string TaxName { get; set; } = string.Empty;
        public string TaxType { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public new bool IsActive { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }

    public class CreateTaxRateDto : CreateDto
    {
        public string TaxCode { get; set; } = string.Empty;
        public string TaxName { get; set; } = string.Empty;
        public string TaxType { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public new bool IsActive { get; set; } = true;
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }

    public class UpdateTaxRateDto : UpdateDto
    {
        public string TaxCode { get; set; } = string.Empty;
        public string TaxName { get; set; } = string.Empty;
        public string TaxType { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public new bool IsActive { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }
}