using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Domain.Finance;

[Table("TaxTransactions")]
public class TaxTransaction
{
    [Key]
    public Guid TaxTransactionId { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
    
    [Required]
    public Guid TaxCodeId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string EntityReferenceTable { get; set; } = string.Empty;
    
    [Required]
    public Guid EntityReferenceId { get; set; }
    
    [Column(TypeName = "decimal(28,8)")]
    public decimal TaxAmount { get; set; }
    
    [Column(TypeName = "decimal(28,8)")]
    public decimal TaxableAmount { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid? CreatedBy { get; set; }
    
    // Navigation properties
    public TaxCode TaxCode { get; set; } = null!;
}
