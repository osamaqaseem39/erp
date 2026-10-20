using ERP.Domain;

namespace ERP.Domain.Finance;

public class BudgetLine : BaseEntity
{
    public Guid BudgetId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal Amount { get; set; }

    public Budget Budget { get; set; } = null!;
}
