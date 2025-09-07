using ERP.Domain;

namespace ERP.Domain.Core;

public class Plan : BaseEntity
{
    public string PlanName { get; set; } = string.Empty; // Basic, Pro, Enterprise
    public string Description { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public int MaxUsers { get; set; }
    public int MaxStorage { get; set; } // in GB
    public string FeaturesJSON { get; set; } = string.Empty; // flexible feature list storage
    public new bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
