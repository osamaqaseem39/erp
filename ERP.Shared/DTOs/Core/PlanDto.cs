using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Core;

public class PlanDto : BaseDto
{
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string BillingCycle { get; set; } = "Monthly";
    public int MaxUsers { get; set; }
    public int MaxStorage { get; set; } // in GB
    public string Features { get; set; } = string.Empty; // JSON string of features
    public bool IsActive { get; set; }
    public int TrialDays { get; set; }
}

public class CreatePlanDto : CreateDto
{
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; } = "USD";
    public string? BillingCycle { get; set; } = "Monthly";
    public int? MaxUsers { get; set; }
    public int? MaxStorage { get; set; }
    public string? Features { get; set; }
    public bool? IsActive { get; set; } = true;
    public int? TrialDays { get; set; }
}

public class UpdatePlanDto : UpdateDto
{
    public string? PlanName { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public string? BillingCycle { get; set; }
    public int? MaxUsers { get; set; }
    public int? MaxStorage { get; set; }
    public string? Features { get; set; }
    public bool? IsActive { get; set; }
    public int? TrialDays { get; set; }
}