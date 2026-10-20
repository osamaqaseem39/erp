using ERP.Domain;

namespace ERP.Domain.Sales;

public class DispatchAdviceLine : BaseEntity
{
    public Guid DispatchAdviceId { get; set; }
    public Guid? SalesOrderLineId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }

    public DispatchAdvice DispatchAdvice { get; set; } = null!;
}
