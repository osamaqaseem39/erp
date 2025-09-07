using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance;

public class ExchangeRateDto : BaseDto
{
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string FromCurrencyName { get; set; } = string.Empty;
    public string ToCurrencyName { get; set; } = string.Empty;
}

public class CreateExchangeRateDto : CreateDto
{
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateExchangeRateDto : UpdateDto
{
    public string? FromCurrency { get; set; }
    public string? ToCurrency { get; set; }
    public decimal? Rate { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool? IsActive { get; set; }
}