using ERP.Domain;

namespace ERP.Domain.Finance;

public class BankReconciliationLine : BaseEntity
{
    public Guid ReconciliationId { get; set; }
    public Guid BankTransactionId { get; set; }
    public decimal MatchedAmount { get; set; }
    public bool IsMatched { get; set; } = true;
    public string? Notes { get; set; }
}
