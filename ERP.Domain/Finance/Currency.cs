using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Domain.Finance;

[Table("Currencies")]
public class Currency
{
    [Key]
    [MaxLength(10)]
    public string CurrencyCode { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string CurrencyName { get; set; } = string.Empty;
    
    [MaxLength(3)]
    public string? Iso4217 { get; set; }
    
    [MaxLength(10)]
    public string? Symbol { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<ExchangeRate> FromExchangeRates { get; set; } = new List<ExchangeRate>();
    public ICollection<ExchangeRate> ToExchangeRates { get; set; } = new List<ExchangeRate>();
}
