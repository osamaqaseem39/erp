using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.CRM;

public class CampaignDto : BaseDto
{
    public Guid? CompanyId { get; set; }
    public string? CampaignName { get; set; }
    public string? CampaignType { get; set; }
    public string? Status { get; set; } = "Draft";
    public DateTime? StartDate { get; set; }
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
    public new bool? IsActive { get; set; } = true;
}

public class CreateCampaignDto : CreateDto
{
    public Guid? CompanyId { get; set; }
    public string? CampaignName { get; set; }
    public string? CampaignType { get; set; }
    public string? Status { get; set; } = "Draft";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public decimal? ActualCost { get; set; }
    public int? ExpectedResponse { get; set; }
    public int? ActualResponse { get; set; }
    public string? Description { get; set; }
    public string? Currency { get; set; }
    public string? TargetAudience { get; set; }
    public string? Goals { get; set; }
}

public class UpdateCampaignDto : UpdateDto
{
    public Guid? CompanyId { get; set; }
    public string? CampaignName { get; set; }
    public string? CampaignType { get; set; }
    public string? Status { get; set; } = "Draft";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public decimal? ActualCost { get; set; }
    public int? ExpectedResponse { get; set; }
    public int? ActualResponse { get; set; }
    public string? Description { get; set; }
    public string? Currency { get; set; }
    public string? TargetAudience { get; set; }
    public string? Goals { get; set; }
}