using ERP.Domain;
using ERP.Domain.Core;

namespace ERP.Domain.CRM;

public class Lead : BaseEntity
{
    public string LeadCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string LeadSource { get; set; } = string.Empty; // Website, Referral, Cold Call, etc.
    public string Source { get; set; } = string.Empty; // Website, Referral, Cold Call, etc.
    public string Status { get; set; } = "New"; // New, Qualified, Converted, Lost
    public string Priority { get; set; } = "Medium"; // Low, Medium, High
    public decimal? EstimatedValue { get; set; }
    public DateTime? LastContactDate { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? AssignedToId { get; set; }
    public Guid? CompanyId { get; set; }
    public string? ContactPerson { get; set; }
    public string? PostalCode { get; set; }
    public string? Industry { get; set; }
    public Guid? AssignedTo { get; set; }
    public int? LeadScore { get; set; }
    public string? Notes { get; set; }
    public DateTime? ConvertedDate { get; set; }
    
    // Navigation properties
    public User? AssignedToUser { get; set; }
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
