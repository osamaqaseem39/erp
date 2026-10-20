using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Domain.Finance;

[Table("BankReconciliations")]
public class BankReconciliation
{
    [Key]
    public Guid ReconciliationId { get; set; }
    
    [Required]
    public Guid BankAccountId { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
    
    [Required]
    public DateTime ReconciliationDate { get; set; }
    
    [Column(TypeName = "decimal(28,8)")]
    public decimal OpeningBalance { get; set; }
    
    [Column(TypeName = "decimal(28,8)")]
    public decimal ClosingBalance { get; set; }
    
    [Column(TypeName = "decimal(28,8)")]
    public decimal Difference { get; set; }
    
    [MaxLength(20)]
    public string Status { get; set; } = "Open"; // Open, Closed
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid? CreatedBy { get; set; }
    
    public DateTime? ClosedAt { get; set; }
    
    public Guid? ClosedBy { get; set; }
    
    // Navigation properties
    public BankAccount BankAccount { get; set; } = null!;
    public ICollection<BankTransaction> BankTransactions { get; set; } = new List<BankTransaction>();
}
