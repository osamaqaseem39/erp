using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Core;

public class SubscriptionDto : BaseDto
{
    public Guid TenantId { get; set; }
    public string? TenantName { get; set; }
    public Guid PlanId { get; set; }
    public string? PlanName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Active";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string BillingCycle { get; set; } = "Monthly";
    public DateTime? NextBillingDate { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSubscriptionDto : CreateDto
{
    public Guid TenantId { get; set; }
    public Guid PlanId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; } = "Active";
    public decimal? Amount { get; set; }
    public string? Currency { get; set; } = "USD";
    public string? BillingCycle { get; set; } = "Monthly";
    public DateTime? NextBillingDate { get; set; }
}

public class UpdateSubscriptionDto : UpdateDto
{
    public Guid? PlanId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public string? BillingCycle { get; set; }
    public DateTime? NextBillingDate { get; set; }
    public bool? IsActive { get; set; }
}