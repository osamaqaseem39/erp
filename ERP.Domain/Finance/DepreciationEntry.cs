using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Domain.Finance;

[Table("DepreciationEntries")]
public class DepreciationEntry
{
    [Key]
    public Guid DepreciationId { get; set; }
    
    [Required]
    public Guid AssetId { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
    
    [Required]
    public DateTime PeriodStart { get; set; }
    
    [Required]
    public DateTime PeriodEnd { get; set; }
    
    [Column(TypeName = "decimal(28,8)")]
    public decimal DepreciationAmount { get; set; }
    
    public Guid? PostedJournalId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid? CreatedBy { get; set; }
    
    // Navigation properties
    public FixedAsset Asset { get; set; } = null!;
    public Journal? PostedJournal { get; set; }
}
