using ERP.Domain;

namespace ERP.Domain.Finance;

public class CostCenter : BaseEntity
{
    public string CostCenterCode { get; set; } = string.Empty;
    public string CostCenterName { get; set; } = string.Empty;
    public Guid CostCenterId { get; set; }
    public string? Description { get; set; }
    public new bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
}