using ERP.Domain;
using ERP.Domain.Core;

namespace ERP.Domain.CRM;

public class Opportunity : BaseEntity
{
    public string OpportunityCode { get; set; } = string.Empty;
    public string OpportunityName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public Guid? LeadId { get; set; }
    public string Stage { get; set; } = "Prospecting"; // Prospecting, Qualification, Proposal, Negotiation, Closed Won, Closed Lost
    public string Status { get; set; } = "Open"; // Open, Won, Lost, Cancelled
    public decimal EstimatedValue { get; set; }
    public decimal? ActualValue { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal Probability { get; set; } = 0; // 0-100
    public DateTime? ExpectedCloseDate { get; set; }
    public DateTime? ActualCloseDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public DateTime? ConvertedDate { get; set; }
    public string? CloseReason { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public string? Source { get; set; }
    public Guid? AssignedTo { get; set; }
    public Guid? AssignedToUserId { get; set; }
    
    // Navigation properties
    public Customer? Customer { get; set; }
    public Lead? Lead { get; set; }
    public User? AssignedToUser { get; set; }
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
