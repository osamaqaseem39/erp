using ERP.Domain;

namespace ERP.Domain.Projects;

public class ProjectMilestone : BaseEntity
{
    public Guid ProjectId { get; set; }
    public string MilestoneName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? TargetDate { get; set; }
    public DateTime? ActualDate { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Completed, Overdue
    public string? Notes { get; set; }
    public decimal Amount { get; set; }
    public bool IsBillable { get; set; }
    public Guid? SalesInvoiceId { get; set; }
    public Guid? FinanceInvoiceId { get; set; }
    public DateTime? BilledAt { get; set; }
    
    // Navigation properties
    public Project Project { get; set; } = null!;
}