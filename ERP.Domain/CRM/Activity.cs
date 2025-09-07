using ERP.Domain;
using ERP.Domain.Core;

namespace ERP.Domain.CRM;

public class Activity : BaseEntity
{
    public string ActivityType { get; set; } = string.Empty; // Call, Email, Meeting, Task
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? LeadId { get; set; }
    public Guid? OpportunityId { get; set; }
    public Guid AssignedToUserId { get; set; }
    public string Status { get; set; } = "Open"; // Open, Completed, Cancelled
    public string Priority { get; set; } = "Medium"; // Low, Medium, High
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    
    // Navigation properties
    public User AssignedToUser { get; set; } = null!;
    public Customer? Customer { get; set; }
    public Lead? Lead { get; set; }
    public Opportunity? Opportunity { get; set; }
}
