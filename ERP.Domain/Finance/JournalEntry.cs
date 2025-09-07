using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Domain.Finance;

[Table("JournalEntries")]
public class JournalEntry
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid EntryId { get; set; }
    
    [Required]
    public Guid JournalId { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
    
    [Required]
    public Guid AccountId { get; set; }
    
    public Guid? CostCenterId { get; set; }
    
    public Guid? ProjectId { get; set; }
    
    [Column(TypeName = "decimal(28,8)")]
    public decimal Debit { get; set; } = 0;
    
    [Column(TypeName = "decimal(28,8)")]
    public decimal Credit { get; set; } = 0;
    
    [MaxLength(1000)]
    public string? Narration { get; set; }
    
    [MaxLength(100)]
    public string? EntityReferenceTable { get; set; }
    
    public Guid? EntityReferenceId { get; set; }
    
    public DateTime EntryDate { get; set; } = DateTime.UtcNow;
    
    [MaxLength(100)]
    public string? Reference { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid? CreatedBy { get; set; }
    
    // Navigation properties
    public Journal Journal { get; set; } = null!;
    public ChartOfAccount Account { get; set; } = null!;
    public CostCenter? CostCenter { get; set; }
}
