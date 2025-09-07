using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Domain.Finance;

[Table("BankTransactions")]
public class BankTransaction
{
    [Key]
    public Guid BankTxId { get; set; }
    
    [Required]
    public Guid BankAccountId { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
    
    [MaxLength(200)]
    public string? BankReference { get; set; }
    
    [Required]
    public DateTime TransactionDate { get; set; }
    
    [Column(TypeName = "decimal(28,8)")]
    public decimal Amount { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string CurrencyCode { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
    
    public Guid? MatchedPaymentId { get; set; }
    
    public bool IsReconciled { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid? CreatedBy { get; set; }
    
    // Navigation properties
    public BankAccount BankAccount { get; set; } = null!;
    public Payment? MatchedPayment { get; set; }
}
