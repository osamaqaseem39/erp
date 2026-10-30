using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.CRM;

public class LeadDto : BaseDto
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
    public string LeadSource { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Status { get; set; } = "New";
    public string Priority { get; set; } = "Medium";
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
    public string? LostReason { get; set; }
    public string? LostNote { get; set; }
    public new bool IsActive { get; set; } = true;
    
    // Navigation properties
    public string? AssignedToUserName { get; set; }
    public string? AssignedToName { get; set; }
}

public class CreateLeadDto : CreateDto
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
    public string LeadSource { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Status { get; set; } = "New";
    public string Priority { get; set; } = "Medium";
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
    /// <summary>Save even when the email or phone matches an existing lead, contact or customer.</summary>
    public bool AllowDuplicate { get; set; }
}

public class UpdateLeadDto : UpdateDto
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
    public string LeadSource { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Status { get; set; } = "New";
    public string Priority { get; set; } = "Medium";
    public decimal? EstimatedValue { get; set; }
    public DateTime? LastContactDate { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? ContactPerson { get; set; }
    public string? PostalCode { get; set; }
    public string? Industry { get; set; }
    public Guid? AssignedTo { get; set; }
    public int? LeadScore { get; set; }
    public string? Notes { get; set; }
}

public class LoseLeadDto
{
    public string LostReason { get; set; } = string.Empty;
    public string? LostNote { get; set; }
}

public class ConvertLeadToOpportunityDto
{
    public string? OpportunityName { get; set; }
    public decimal? Amount { get; set; }
    public decimal? Probability { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public Guid? CustomerId { get; set; }
}

public class LeadDuplicateDto
{
    public string Kind { get; set; } = string.Empty; // Lead, Customer, Contact
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
