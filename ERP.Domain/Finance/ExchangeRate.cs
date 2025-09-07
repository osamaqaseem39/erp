using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Domain.Finance;

[Table("ExchangeRates")]
public class ExchangeRate
{
    [Key]
    public Guid ExchangeRateId { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string FromCurrency { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(10)]
    public string ToCurrency { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,8)")]
    public decimal Rate { get; set; }
    
    [Required]
    public DateTime EffectiveDate { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid? CreatedBy { get; set; }
    
    // Navigation properties
    public Currency FromCurrencyNavigation { get; set; } = null!;
    public Currency ToCurrencyNavigation { get; set; } = null!;
}
