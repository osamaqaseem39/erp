using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance;

public class CurrencyDto : BaseDto
{
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencyName { get; set; } = string.Empty;
    public string Iso4217 { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public int DecimalPlaces { get; set; } = 2;
    public bool IsBaseCurrency { get; set; } = false;
    public bool IsActive { get; set; } = true;
}

public class CreateCurrencyDto : CreateDto
{
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencyName { get; set; } = string.Empty;
    public string Iso4217 { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public int DecimalPlaces { get; set; } = 2;
    public bool IsBaseCurrency { get; set; } = false;
    public bool IsActive { get; set; } = true;
}

public class UpdateCurrencyDto : UpdateDto
{
    public string? CurrencyCode { get; set; }
    public string? CurrencyName { get; set; }
    public string? Iso4217 { get; set; }
    public string? Symbol { get; set; }
    public int? DecimalPlaces { get; set; }
    public bool? IsBaseCurrency { get; set; }
    public bool? IsActive { get; set; }
}