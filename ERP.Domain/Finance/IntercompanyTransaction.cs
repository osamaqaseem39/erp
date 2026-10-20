using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Domain.Finance;

[Table("IntercompanyTransactions")]
public class IntercompanyTransaction
{
    [Key]
    public Guid IntercompanyId { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
    
    [Required]
    public Guid FromCompanyId { get; set; }
    
    [Required]
    public Guid ToCompanyId { get; set; }
    
    [Column(TypeName = "decimal(28,8)")]
    public decimal Amount { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string CurrencyCode { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [MaxLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Posted

    public Guid? SettlementId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid? CreatedBy { get; set; }

    public IntercompanySettlement? Settlement { get; set; }
}
