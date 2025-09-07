using ERP.Domain;

namespace ERP.Domain.Finance;

public class Budget : BaseEntity
{
    public Guid AccountId { get; set; }
    public Guid BudgetId { get; set; }
    public string BudgetName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime BudgetPeriodStart { get; set; }
    public DateTime BudgetPeriodEnd { get; set; }
    public decimal BudgetAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft"; // Draft, Active, Closed
    
    // Navigation properties
    public ChartOfAccount Account { get; set; } = null!;
}