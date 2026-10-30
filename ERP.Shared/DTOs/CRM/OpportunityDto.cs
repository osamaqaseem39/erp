using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.CRM;

public class OpportunityDto : BaseDto
{
    public string? OpportunityCode { get; set; }
    public string? OpportunityName { get; set; }
    public string? Title { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? LeadId { get; set; }
    public string? Stage { get; set; } = "Prospecting";
    public string? Status { get; set; } = "Open";
    public decimal? EstimatedValue { get; set; }
    public decimal? ActualValue { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; } = "USD";
    public decimal? Probability { get; set; } = 0;
    public DateTime? ExpectedCloseDate { get; set; }
    public DateTime? ActualCloseDate { get; set; }
    public DateTime? ConvertedDate { get; set; }
    public string? CloseReason { get; set; }
    /// <summary>Amount x probability: the pipeline value this deal is expected to bring in.</summary>
    public decimal? WeightedValue { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public string? Source { get; set; }
    public Guid? AssignedTo { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public new bool? IsActive { get; set; } = true;
    public string? CustomerName { get; set; }
    public string? LeadName { get; set; }
    public string? AssignedToName { get; set; }
    public string? AssignedToUserName { get; set; }
}

public class CreateOpportunityDto : CreateDto
{
    public string? OpportunityCode { get; set; }
    public string? OpportunityName { get; set; }
    public string? Title { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? LeadId { get; set; }
    public string? Stage { get; set; } = "Prospecting";
    public string? Status { get; set; } = "Open";
    public decimal? EstimatedValue { get; set; }
    public decimal? ActualValue { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; } = "USD";
    public decimal? Probability { get; set; } = 0;
    public DateTime? ExpectedCloseDate { get; set; }
    public DateTime? ActualCloseDate { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public string? Source { get; set; }
    public Guid? AssignedTo { get; set; }
    public Guid? AssignedToUserId { get; set; }
}

public class UpdateOpportunityDto : UpdateDto
{
    public string? OpportunityCode { get; set; }
    public string? OpportunityName { get; set; }
    public string? Title { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? LeadId { get; set; }
    public string? Stage { get; set; } = "Prospecting";
    public string? Status { get; set; } = "Open";
    public decimal? EstimatedValue { get; set; }
    public decimal? ActualValue { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; } = "USD";
    public decimal? Probability { get; set; } = 0;
    public DateTime? ExpectedCloseDate { get; set; }
    public DateTime? ActualCloseDate { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public string? Source { get; set; }
    public Guid? AssignedTo { get; set; }
    public Guid? AssignedToUserId { get; set; }
}

public class ChangeOpportunityStageDto
{
    public string Stage { get; set; } = string.Empty;
    /// <summary>Leave empty to use the stage default (10/25/50/75).</summary>
    public decimal? Probability { get; set; }
}
