using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Domain.Finance;

[Table("BillLines")]
public class BillLine
{
    [Key]
    public Guid BillLineId { get; set; }
    
    [Required]
    public Guid BillId { get; set; }
    
    public Guid? ProductId { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }
    
    [Column(TypeName = "decimal(28,8)")]
    public decimal UnitPrice { get; set; }
    
    [Column(TypeName = "decimal(28,8)")]
    public decimal LineTotal { get; set; }
    
    [Column(TypeName = "decimal(10,4)")]
    public decimal TaxRate { get; set; } = 0;
    
    [Required]
    public Guid GLAccountId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid? CreatedBy { get; set; }
    
    // Navigation properties
    public Bill Bill { get; set; } = null!;
    public ChartOfAccount GLAccount { get; set; } = null!;
}
