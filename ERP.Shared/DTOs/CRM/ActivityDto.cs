using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.CRM;

public class ActivityDto : BaseDto
{
    public string? Type { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Open";
    public string Priority { get; set; } = "Medium";
    public Guid AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public string? AssignedTo { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public Guid? RelatedToId { get; set; }
    public string? RelatedEntityType { get; set; }
    public string? RelatedToType { get; set; }
    
    // Navigation properties for display
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid? LeadId { get; set; }
    public string? LeadName { get; set; }
    public Guid? OpportunityId { get; set; }
    public string? OpportunityName { get; set; }
}

public class CreateActivityDto : CreateDto
{
    public string ActivityType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Open";
    public string Priority { get; set; } = "Medium";
    public Guid AssignedToUserId { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? LeadId { get; set; }
    public Guid? OpportunityId { get; set; }
}

public class UpdateActivityDto : UpdateDto
{
    public string ActivityType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Open";
    public string Priority { get; set; } = "Medium";
    public Guid AssignedToUserId { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? LeadId { get; set; }
    public Guid? OpportunityId { get; set; }
}