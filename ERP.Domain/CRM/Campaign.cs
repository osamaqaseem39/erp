using ERP.Domain;

namespace ERP.Domain.CRM;

public class Campaign : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public string CampaignType { get; set; } = string.Empty; // Email, SMS, Social Media, Print, etc.
    public string Status { get; set; } = "Draft"; // Draft, Active, Paused, Completed, Cancelled
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public decimal? ActualCost { get; set; }
    public int? ExpectedResponse { get; set; }
    public int? ActualResponse { get; set; }
    public string? Description { get; set; }
    public string? Currency { get; set; }
    public string? TargetAudience { get; set; }
    public string? Goals { get; set; }
    public DateTime? LaunchDate { get; set; }
    public DateTime? PausedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
}
